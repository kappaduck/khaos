// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed record EngineArguments(
    Game2D Game,
    Window Window,
    WindowOptions WindowOptions,
    GraphicsOptions Graphics,
    EngineOptions Engine,
    IDisposable? Services,
    ILoggerFactory? Loggers);
