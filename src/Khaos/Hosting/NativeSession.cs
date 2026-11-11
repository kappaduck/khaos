// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;

namespace KappaDuck.Khaos.Hosting;

internal sealed class NativeSession : IDisposable
{
    private const string AppMetadataCopyrightProperty = "SDL.app.metadata.copyright";
    private const string AppMetadataCreatorProperty = "SDL.app.metadata.creator";
    private const string AppMetadataIdentifierProperty = "SDL.app.metadata.identifier";
    private const string AppMetadataNameProperty = "SDL.app.metadata.name";
    private const string AppMetadataTypeProperty = "SDL.app.metadata.type";
    private const string AppMetadataUrlProperty = "SDL.app.metadata.url";
    private const string AppMetadataVersionProperty = "SDL.app.metadata.version";
    private const string NoSignalHandlersHint = "SDL_NO_SIGNAL_HANDLERS";

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

    internal static NativeSession Start(AppMetadata metadata)
    {
        if (Interlocked.CompareExchange(ref _started, true, false))
            throw new InvalidOperationException("The native session is already started. Dispose it before starting it again.");

        try
        {
            SetMetadata(metadata);

            if (!OperatingSystem.IsBrowser())
                SDL3.ThrowIfFailed(SDL3.SetHint(NoSignalHandlersHint, "1"));

            SDL3.ThrowIfFailed(SDL3.Init(SDL_Subsystems.Video));
        }
        catch
        {
            Stop();
            throw;
        }

        return new NativeSession();
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
