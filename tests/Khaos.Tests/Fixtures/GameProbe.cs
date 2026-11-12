// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Fixtures;

internal sealed class GameProbe
{
    internal Window? CameraWindow { get; set; }

    internal bool Created { get; set; }

    internal bool Disposed { get; set; }

    internal Window? GameWindow { get; set; }

    internal bool ShutDown { get; set; }

    internal string? Title { get; set; }

    internal string? TitleAtShutdown { get; set; }

    internal int? VSync { get; set; }
}
