// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.Browser;
using System.Runtime.ExceptionServices;

namespace KappaDuck.Khaos.Hosting;

internal static class GameRunner
{
    private static Game2D? _game;
    private static ExceptionDispatchInfo? _failure;

    internal static int Run(Game2D game)
    {
        if (NativeSession.Started)
            throw new InvalidOperationException("A game is already running. Only one game runs at a time.");

        _failure = null;
        _game = game;

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

        TakeFailure()?.Throw();

        return exitCode;
    }

    [UnmanagedCallersOnly]
    private static SDL_AppResult AppEvent(void* state, SDL_Event* e)
    {
        KhaosEngine engine = From(state);

        try
        {
            if (unsafe (e->Type) == SDL_EventType.Quit)
                engine.Game.Exit();

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
        if (_game is not { } game)
            return SDL_AppResult.Failure;

        _game = null;

        try
        {
            KhaosEngine engine = KhaosEngine.Start(game);

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

    private static ExceptionDispatchInfo? TakeFailure()
    {
        ExceptionDispatchInfo? failure = _failure;
        _failure = null;
        return failure;
    }
}
