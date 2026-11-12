// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed class NativeSession : IDisposable
{
    private static bool _started;
    private bool _disposed;

    private NativeSession()
    {
    }

    internal static bool Started => _started;

    public void Dispose()
    {
        if (!Interlocked.Exchange(ref _disposed, true))
            Stop();
    }

    internal static NativeSession Start(AppMetadata metadata, EngineOptions? engine = null, ILogger? logger = null)
    {
        if (Interlocked.CompareExchange(ref _started, true, false))
            throw new InvalidOperationException("The native session is already started. Dispose it before starting it again.");

        logger ??= NullLogger.Instance;

        try
        {
            SetMetadata(metadata);

            VideoDriver chosen = engine is null ? VideoDriver.Auto : ChooseVideoDriver(engine.VideoDriver, logger);

            if (engine is not null)
                NativeHints.Apply(engine.Native, logger);

            if (!OperatingSystem.IsBrowser())
                NativeHints.Force(SDL3.NoSignalHandlers, "1");

            Init(chosen, logger);
        }
        catch
        {
            Stop();
            throw;
        }

        return new NativeSession();
    }

    private static VideoDriver ChooseVideoDriver(VideoDriver driver, ILogger logger)
    {
        if (driver == VideoDriver.Auto)
            return VideoDriver.Auto;

        return NativeHints.Set(SDL3.VideoDriver, NativeHints.NameOf(driver), logger) ? driver : VideoDriver.Auto;
    }

    private static void Init(VideoDriver chosen, ILogger logger)
    {
        if (chosen != VideoDriver.Auto)
        {
            if (SDL3.Init(SDL_Subsystems.Video))
                return;

            NativeHints.DriverMissing(logger, NativeHints.VideoDriverSetting, NativeHints.NameOf(chosen), SDL3.GetError());
            NativeHints.Reset(SDL3.VideoDriver);
        }

        SDL3.ThrowIfFailed(SDL3.Init(SDL_Subsystems.Video));
    }

    private static void SetMetadata(AppMetadata metadata)
    {
        Set(SDL3.AppMetadataName, metadata.Name);
        Set(SDL3.AppMetadataVersion, metadata.Version);
        Set(SDL3.AppMetadataIdentifier, metadata.Identifier);
        Set(SDL3.AppMetadataCreator, metadata.Creator);
        Set(SDL3.AppMetadataCopyright, metadata.Copyright);
        Set(SDL3.AppMetadataUrl, metadata.Url?.AbsoluteUri);
        Set(SDL3.AppMetadataType, "game");

        static void Set(string property, string? value)
        {
            if (value is null)
                return;

            SDL3.ThrowIfFailed(SDL3.SetAppMetadataProperty(property, value));
        }
    }

    private static void Stop()
    {
        SDL3.Quit();
        CustomEventRegistry.ReleaseAll();
        Interlocked.Exchange(ref _started, false);
    }
}
