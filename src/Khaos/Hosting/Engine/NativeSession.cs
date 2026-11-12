// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed class NativeSession : IDisposable
{
    private const string AppMetadataCopyrightProperty = "SDL.app.metadata.copyright";
    private const string AppMetadataCreatorProperty = "SDL.app.metadata.creator";
    private const string AppMetadataIdentifierProperty = "SDL.app.metadata.identifier";
    private const string AppMetadataNameProperty = "SDL.app.metadata.name";
    private const string AppMetadataTypeProperty = "SDL.app.metadata.type";
    private const string AppMetadataUrlProperty = "SDL.app.metadata.url";
    private const string AppMetadataVersionProperty = "SDL.app.metadata.version";

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
                NativeHints.Force(NativeHints.NoSignalHandlers, "1");

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

        return NativeHints.Set(NativeHints.VideoDriverHint, NativeHints.NameOf(driver), logger) ? driver : VideoDriver.Auto;
    }

    private static void Init(VideoDriver chosen, ILogger logger)
    {
        if (chosen != VideoDriver.Auto)
        {
            if (SDL3.Init(SDL_Subsystems.Video))
                return;

            NativeHints.DriverMissing(logger, NativeHints.VideoDriverSetting, NativeHints.NameOf(chosen), SDL3.GetError());
            NativeHints.Reset(NativeHints.VideoDriverHint);
        }

        SDL3.ThrowIfFailed(SDL3.Init(SDL_Subsystems.Video));
    }

    private static void SetMetadata(AppMetadata metadata)
    {
        Set(AppMetadataNameProperty, metadata.Name);
        Set(AppMetadataVersionProperty, metadata.Version);
        Set(AppMetadataIdentifierProperty, metadata.Identifier);
        Set(AppMetadataCreatorProperty, metadata.Creator);
        Set(AppMetadataCopyrightProperty, metadata.Copyright);
        Set(AppMetadataUrlProperty, metadata.Url?.AbsoluteUri);
        Set(AppMetadataTypeProperty, "game");

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
