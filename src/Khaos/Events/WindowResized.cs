// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Events;

/// <summary>
/// The window changed size.
/// </summary>
/// <param name="Size">The new size of the inside of the window, in logical units, like <see cref="Window.Size"/>.</param>
[KhaosEvent(SDL_EventType.WindowResized)]
public readonly record struct WindowResized(SizeI Size)
{
    internal WindowResized(in SDL_WindowEvent native, in EventContext context) : this(WindowScale.ToLogical(new SizeI(native.Data1, native.Data2), context.ContentScale))
    {
    }
}
