// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Tests.Fixtures;

internal sealed class OptionsReadingGame : Game2D
{
    public OptionsReadingGame(IOptions<WindowOptions> options) : base(320, 180) => _ = options.Value;

    protected internal override void Draw(Renderer renderer) => Exit();
}
