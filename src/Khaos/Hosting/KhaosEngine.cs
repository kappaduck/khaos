// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Hosting;

internal sealed class KhaosEngine : IDisposable
{
    private readonly NativeSession _session;
    private readonly Renderer _renderer;

    private KhaosEngine(Game2D game)
    {
        Game = game;
        AppMetadata metadata = AppMetadata.FromAppContext();

        _session = NativeSession.Start(metadata);

        try
        {
            Window = Window.Open(game.WindowOptions, metadata.Name);

            try
            {
                _renderer = Renderer.Create(Window, game.GraphicsOptions.VSync, NullLogger<Renderer>.Instance);
            }
            catch
            {
                Window.Close();
                throw;
            }
        }
        catch
        {
            _session.Dispose();
            throw;
        }
    }

    internal Game2D Game { get; }

    internal Window Window { get; }

    public void Dispose()
    {
        _renderer.Destroy();
        Window.Close();

        _session.Dispose();
    }

    internal SDL_AppResult Iterate()
    {
        if (Game.ExitRequested)
            return SDL_AppResult.Success;

        Game.Draw(_renderer);
        _renderer.Present();

        return SDL_AppResult.Continue;
    }

    internal static KhaosEngine Start(Game2D game)
    {
        KhaosEngine engine = new(game);
        game.Reset();

        try
        {
            game.Initialize();
        }
        catch
        {
            engine.Dispose();
            throw;
        }

        return engine;
    }
}
