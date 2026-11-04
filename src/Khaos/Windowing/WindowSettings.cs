// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Windowing;

internal sealed class WindowSettings
{
    public bool? Borderless { get; set; }

    public bool? Fullscreen { get; set; }

    public int? Height { get; set; }

    public bool? Resizable { get; set; }

    public string? Title { get; set; }

    public int? Width { get; set; }

    internal void ApplyTo(WindowOptions options)
    {
        options.Title = Title ?? options.Title;
        options.Width = Width ?? options.Width;
        options.Height = Height ?? options.Height;
        options.Fullscreen = Fullscreen ?? options.Fullscreen;
        options.Resizable = Resizable ?? options.Resizable;
        options.Borderless = Borderless ?? options.Borderless;
    }
}
