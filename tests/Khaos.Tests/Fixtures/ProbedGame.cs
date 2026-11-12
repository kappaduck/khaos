// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics.Rendering;

namespace KappaDuck.Khaos.Tests.Fixtures;

internal sealed class ProbedGame : Game2D, IDisposable
{
    private readonly Camera? _camera;
    private readonly GameProbe _probe;

    public ProbedGame(GameProbe probe, Camera? camera = null) : base("Hosted", 320, 180)
    {
        _camera = camera;
        _probe = probe;
        _probe.Created = true;
    }

    public void Dispose() => _probe.Disposed = true;

    protected internal override void Draw(Renderer renderer)
    {
        _probe.VSync = SDLRender.ReadVSync(renderer);
        Exit();
    }

    protected internal override void Initialize()
    {
        _probe.CameraWindow = _camera?.Window;
        _probe.GameWindow = Window;
        _probe.Title = Window.Title;
    }

    protected internal override void Shutdown()
    {
        _probe.ShutDown = true;
        _probe.TitleAtShutdown = Window.Title;
    }
}
