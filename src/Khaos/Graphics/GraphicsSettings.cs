// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Graphics;

internal sealed class GraphicsSettings
{
    public string? VSync { get; set; }

    internal void ApplyTo(GraphicsOptions options)
    {
        if (VSync is null)
            return;

        if (!Graphics.VSync.TryParse(VSync, out VSync vsync))
            throw new KhaosException($"The setting {GraphicsOptions.Section}:vsync must be true, false, adaptive or a number of refreshes, but it is '{VSync}'.");

        options.VSync = vsync;
    }
}
