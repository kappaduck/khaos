// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.Browser;
using System.Runtime.ExceptionServices;

namespace KappaDuck.Khaos.Hosting.Engine;

internal static class GameRunner
{
    private static ExceptionDispatchInfo? _failure;
    private static EngineArguments? _arguments;

    internal static int Run(EngineArguments arguments)
    {
        try
        {
            ThrowIfAGameRuns();
        }
        catch
        {
            arguments.Services?.Dispose();
            throw;
        }

        _failure = null;
        _arguments = arguments;

        int exitCode;

        if (OperatingSystem.IsBrowser())
        {
            exitCode = EnterCallbacks();
        }
        else
        {
            using ShutdownSignals signals = new();
            exitCode = EnterCallbacks();
        }

        TakeArguments()?.Services?.Dispose();
        TakeFailure()?.Throw();

        return exitCode;
    }

    internal static void ThrowIfAGameRuns()
    {
        if (NativeSession.Started)
            throw new InvalidOperationException("A game is already running. Only one game runs at a time.");
    }

    [UnmanagedCallersOnly]
    private static SDL_AppResult AppEvent(void* state, SDL_Event* e)
    {
        KhaosEngine engine = From(state);

        try
        {
            // SAFETY: SDL passes an event it owns, valid until the callback returns, and Receive copies what it keeps.
            unsafe
            {
                engine.Receive(in *e);
            }

            return SDL_AppResult.Continue;
        }
        catch (Exception ex)
        {
            return FailInLoop(ex);
        }
    }

    [UnmanagedCallersOnly]
    private static SDL_AppResult AppInit(void** state, int argc, byte** argv)
    {
        if (TakeArguments() is not { } arguments)
            return SDL_AppResult.Failure;

        try
        {
            KhaosEngine engine = KhaosEngine.Start(arguments);

            // SAFETY: AppQuit frees the handle once, and SDL reads the app state only between AppInit and AppQuit.
            GCHandle<KhaosEngine> handle = unsafe (new GCHandle<KhaosEngine>(engine));

            // SAFETY: SDL passes the address where it keeps the app state, and reads it only after this callback.
            unsafe
            {
                *state = (void*)GCHandle<KhaosEngine>.ToIntPtr(handle);
            }

            return SDL_AppResult.Continue;
        }
        catch (Exception ex)
        {
            return KeepForRun(ex);
        }
    }

    [UnmanagedCallersOnly]
    private static SDL_AppResult AppIterate(void* state)
    {
        try
        {
            return From(state).Iterate();
        }
        catch (Exception ex)
        {
            return FailInLoop(ex);
        }
    }

    [UnmanagedCallersOnly]
    private static void AppQuit(void* state, SDL_AppResult result)
    {
        if (state is null)
            return;

        // SAFETY: state is the handle AppInit stored, and SDL calls AppQuit once, after the last callback.
        GCHandle<KhaosEngine> handle = unsafe (GCHandle<KhaosEngine>.FromIntPtr((nint)state));

        try
        {
            handle.Target.Dispose();

            if (OperatingSystem.IsBrowser() && result == SDL_AppResult.Success)
                KhaosPage.Ended();
        }
        catch (Exception exception)
        {
            _ = FailInLoop(exception);
        }
        finally
        {
            handle.Dispose();
        }
    }

    private static int EnterCallbacks()
    {
        // SAFETY: the four callbacks match the signatures of SDL_AppInit_func, SDL_AppIterate_func, SDL_AppEvent_func
        // and SDL_AppQuit_func, and a null argv is what SDL passes on when the platform has none.
        unsafe
        {
            return SDL3.EnterAppMainCallbacks(0, null, &AppInit, &AppIterate, &AppEvent, &AppQuit);
        }
    }

    private static SDL_AppResult FailInLoop(Exception exception)
    {
        if (!OperatingSystem.IsBrowser())
            return KeepForRun(exception);

        KhaosPage.Stopped(exception.ToString());
        return SDL_AppResult.Failure;
    }

    // SAFETY: appState is the handle AppInit stored, and AppQuit frees it only after the last callback.
    private static KhaosEngine From(void* state) => unsafe (GCHandle<KhaosEngine>.FromIntPtr((nint)state).Target);

    private static SDL_AppResult KeepForRun(Exception exception)
    {
        _failure ??= ExceptionDispatchInfo.Capture(exception);
        return SDL_AppResult.Failure;
    }

    private static EngineArguments? TakeArguments()
    {
        EngineArguments? arguments = _arguments;
        _arguments = null;
        return arguments;
    }

    private static ExceptionDispatchInfo? TakeFailure()
    {
        ExceptionDispatchInfo? failure = _failure;
        _failure = null;
        return failure;
    }
}
