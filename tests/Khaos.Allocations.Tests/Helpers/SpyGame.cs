// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;

namespace KappaDuck.Khaos.Allocations.Tests.Helpers;

internal sealed class SpyGame : Game2D
{
    internal SpyGame() : base("Duck Quest", 320, 180) => GraphicsOptions.VSync = VSync.Disabled;

    internal int Draws { get; private set; }

    internal Action<SpyGame>? OnDraw { get; init; }

    internal Action<SpyGame, Event>? OnEventReceived { get; init; }

    protected internal override void Draw(Renderer renderer)
    {
        Draws++;
        renderer.Clear(Color.Teal);

        OnDraw?.Invoke(this);
    }

    protected internal override void OnEvent(in Event e) => OnEventReceived?.Invoke(this, e);
}
