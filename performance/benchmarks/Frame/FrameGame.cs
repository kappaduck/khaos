// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;

namespace KappaDuck.Khaos.Benchmarks.Frame;

internal sealed class FrameGame : Game2D
{
    internal FrameGame() : base("Benchmarks", 1280, 720) => GraphicsOptions.VSync = VSync.Disabled;

    protected internal override void Draw(Renderer renderer) => renderer.Clear(Color.Black);
}
