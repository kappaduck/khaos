// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Benchmarks.Frames;

internal sealed class FrameGame() : Game2D("Benchmarks", 1280, 720)
{
    internal float Height { get; private set; }

    internal int Jumps { get; private set; }

    internal int Score { get; private set; }

    internal double Seconds { get; private set; }

    internal SizeI Size { get; private set; }

    protected internal override void Draw(Renderer renderer, in Frame frame) => renderer.Clear(Color.Black);

    protected internal override void FixedUpdate(in GameTime time) => Height += 9.81f * time.Delta;

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

    protected internal override void Update(in GameTime time) => Seconds = time.Total;
}
