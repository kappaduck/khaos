// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

/// <summary>
/// How the engine starts, set before the game runs.
/// </summary>
/// <remarks>
/// Set them in <c>Program.cs</c> with <c>builder.ConfigureEngine</c>. The <c>engine</c> section of your settings, the
/// command line and the environment variables replace what the code sets.
/// </remarks>
public sealed class EngineOptions
{
    internal const string NativeSection = "native";
    internal const string Section = "engine";

    /// <summary>
    /// Gets the native settings of the engine by name, such as <c>SDL_RENDER_DRIVER</c>. Empty by default.
    /// </summary>
    /// <remarks>
    /// Khaos applies them once, when the game starts. In the settings, the key is <c>engine:native:SDL_RENDER_DRIVER</c>,
    /// and an environment variable of the same name wins over it.
    /// </remarks>
    public IDictionary<string, string> Native { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the graphics API the game draws with. <see cref="RenderDriver.Auto"/> by default.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>engine:renderDriver</c>. When the computer does not support it, Khaos writes a
    /// warning and picks another one.
    /// </remarks>
    public RenderDriver RenderDriver { get; set; }

    /// <summary>
    /// Gets or sets the system the game opens its window with. <see cref="VideoDriver.Auto"/> by default.
    /// </summary>
    /// <remarks>
    /// In the settings, the key is <c>engine:videoDriver</c>. When the computer does not support it, Khaos writes a
    /// warning and picks another one.
    /// </remarks>
    public VideoDriver VideoDriver { get; set; }
}
