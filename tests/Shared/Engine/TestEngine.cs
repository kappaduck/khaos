// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Testing.Engine;

internal static class TestEngine
{
    internal static int Run(Game2D game) => GameRunner.Run(ArgumentsOf(game));

    internal static KhaosEngine Start(Game2D game) => KhaosEngine.Start(ArgumentsOf(game));

    internal static KhaosEngine Start(Game2D game, GraphicsOptions graphics)
        => KhaosEngine.Start(ArgumentsOf(game) with { Graphics = graphics });

    internal static KhaosEngine Start(Game2D game, EngineOptions engine, ILoggerFactory? loggers = null)
        => KhaosEngine.Start(ArgumentsOf(game) with { Engine = engine, Loggers = loggers });

    internal static KhaosEngine Start(Game2D game, ILoggerFactory loggers)
        => KhaosEngine.Start(ArgumentsOf(game) with { Loggers = loggers });

    internal static KhaosEngine Start(Game2D game, WindowOptions options)
        => KhaosEngine.Start(ArgumentsOf(game) with { WindowOptions = options });

    internal static KhaosEngine Start(Game2D game, Window window, IDisposable? services)
        => KhaosEngine.Start(ArgumentsOf(game) with { Window = window, Services = services });

    internal static EngineArguments ArgumentsOf(Game2D game)
    {
        WindowOptions options = new();
        game.ApplyTo(options);

        return new EngineArguments(game, new Window(), options, new GraphicsOptions { VSync = VSync.Disabled }, new EngineOptions(), null, null);
    }
}
