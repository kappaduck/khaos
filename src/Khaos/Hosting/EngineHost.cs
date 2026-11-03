// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting;

internal sealed class EngineHost : IDisposable
{
    private const string AppMetadataCopyrightProperty = "SDL.app.metadata.copyright";
    private const string AppMetadataCreatorProperty = "SDL.app.metadata.creator";
    private const string AppMetadataIdentifierProperty = "SDL.app.metadata.identifier";
    private const string AppMetadataNameProperty = "SDL.app.metadata.name";
    private const string AppMetadataTypeProperty = "SDL.app.metadata.type";
    private const string AppMetadataUrlProperty = "SDL.app.metadata.url";
    private const string AppMetadataVersionProperty = "SDL.app.metadata.version";

    private const SDL_Subsystems Subsystems = SDL_Subsystems.Video;

    private static int _started;
    private int _disposed;

    private EngineHost()
    {
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            Stop();
    }

    internal static EngineHost Start(AppMetadata metadata)
    {
        if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
            throw new InvalidOperationException("The engine host is already started. Dispose it before starting it again.");

        try
        {
            SetMetadata(metadata);
            SDL3.ThrowIfFailed(SDL3.Init(Subsystems));
        }
        catch
        {
            Stop();
            throw;
        }

        return new EngineHost();
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
        Interlocked.Exchange(ref _started, 0);
    }
}
