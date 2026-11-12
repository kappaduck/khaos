// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Hosting;

internal sealed class GameWindowDefaults<TGame>(TGame game) : IConfigureOptions<WindowOptions>
    where TGame : Game2D
{
    public void Configure(WindowOptions options) => game.ApplyTo(options);
}
