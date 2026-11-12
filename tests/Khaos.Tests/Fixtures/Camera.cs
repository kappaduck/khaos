// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Fixtures;

internal sealed class Camera(Window window)
{
    internal Window Window { get; } = window;
}
