// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Hosting;

internal sealed class KhaosEngine : IDisposable
{
    private readonly NativeSession _session;
    private readonly Renderer _renderer;

    private bool _disposed;
    private EventContext _eventContext;

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
                _eventContext = new EventContext(Window.ContentScale);
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
        if (_disposed)
            return;

        _disposed = true;

        _renderer.Destroy();
        Window.Close();

        _session.Dispose();
    }

    internal void Dispatch(in Event e)
    {
        Game.OnEvent(in e);

        if (e is QuitRequested)
            Game.Exit();
    }

    internal SDL_AppResult Iterate()
    {
        if (Game.ExitRequested)
            return SDL_AppResult.Success;

        Game.Draw(_renderer);
        _renderer.Present();

        return SDL_AppResult.Continue;
    }

    internal void Receive(in SDL_Event native)
    {
        if (ChangesContentScale(native.Type))
            _eventContext = new EventContext(Window.ContentScale);


        if (IsKeyRepeat(in native))
            return;

        if (!EventConverter.TryConvert(in native, in _eventContext, out Event e))
            return;

        Dispatch(in e);

        if (e.TryGetValue(out CustomEvent custom))
            custom.Release();
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

    private static bool ChangesContentScale(SDL_EventType type)
        => type is SDL_EventType.WindowResized or SDL_EventType.WindowPixelSizeChanged or SDL_EventType.WindowDisplayScaleChanged;

    private static bool IsKeyRepeat(in SDL_Event native) => native.Type == SDL_EventType.KeyDown && native.Key.Repeat != 0;
}
