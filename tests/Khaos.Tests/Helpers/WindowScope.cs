// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Helpers;

internal sealed class WindowScope : IDisposable
{
    private WindowScope(Window window) => Window = window;

    internal Window Window { get; }

    public void Dispose() => Window.Close();

    internal static WindowScope Open(WindowOptions options, string? defaultTitle = null) => new(Window.Open(options, defaultTitle));
}
