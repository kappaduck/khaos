// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Benchmarks.Frame;

internal sealed class FrameGame : Game2D
{
    internal FrameGame() : base("Benchmarks", 1280, 720) => GraphicsOptions.VSync = VSync.Disabled;

    internal int Jumps { get; private set; }

    internal int Score { get; private set; }

    internal SizeI Size { get; private set; }

    protected internal override void Draw(Renderer renderer) => renderer.Clear(Color.Black);

    protected internal override void OnEvent(in Event e)
    {
        switch (e)
        {
            case KeyPressed { Code: Scancode.Space }:
                Jumps++;
                break;
            case WindowResized resized:
                Size = resized.Size;
                break;
            case CustomEvent { FrameEvents: ScoreChanged changed }:
                Score = changed.Points;
                break;
            default:
                break;
        }
    }
}
