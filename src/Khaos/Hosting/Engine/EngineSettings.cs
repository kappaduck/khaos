// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed class EngineSettings
{
    public string? RenderDriver { get; set; }

    public string? VideoDriver { get; set; }

    internal void ApplyTo(EngineOptions options)
    {
        if (RenderDriver is not null)
            options.RenderDriver = Parse<Engine.RenderDriver>(RenderDriver, "renderDriver", "auto, direct3d11, direct3d12, opengl, opengles2, vulkan, gpu or software");

        if (VideoDriver is not null)
            options.VideoDriver = Parse<Engine.VideoDriver>(VideoDriver, "videoDriver", "auto, wayland, x11 or offscreen");
    }

    private static T Parse<T>(string value, string key, string allowed) where T : struct, Enum
    {
        bool isName = !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);

        if (isName && Enum.TryParse(value, true, out T driver) && Enum.IsDefined(driver))
            return driver;

        throw new KhaosException($"The setting {EngineOptions.Section}:{key} must be {allowed}, but it is '{value}'.");
    }
}
