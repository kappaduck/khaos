// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Collections.Frozen;

namespace KappaDuck.Khaos.Hosting.Engine;

internal static partial class NativeHints
{
    internal const string MainCallbackRate = "SDL_MAIN_CALLBACK_RATE";
    internal const string NoSignalHandlers = "SDL_NO_SIGNAL_HANDLERS";
    internal const string RenderDriverHint = "SDL_RENDER_DRIVER";
    internal const string RenderDriverSetting = "engine:renderDriver";
    internal const string VideoDriverHint = "SDL_VIDEO_DRIVER";
    internal const string VideoDriverSetting = "engine:videoDriver";

    private const string Prefix = "SDL_";

    private static readonly FrozenSet<string> _invariants = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, NoSignalHandlers);

    private static readonly FrozenDictionary<string, string> _ownedByOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [MainCallbackRate] = "graphics:maxFrameRate",
        [RenderDriverHint] = RenderDriverSetting,
        [VideoDriverHint] = VideoDriverSetting
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    internal static void Apply(IDictionary<string, string> native, ILogger logger)
    {
        foreach ((string name, string value) in native)
            _ = Set(name.ToUpperInvariant(), value, logger);
    }

    internal static void Force(string name, string value)
        => SDL3.ThrowIfFailed(SDL3.SetHintWithPriority(name, value, SDL_HintPriority.Override));

    internal static string NameOf(RenderDriver driver) => driver switch
    {
        RenderDriver.Direct3D11 => "direct3d11",
        RenderDriver.Direct3D12 => "direct3d12",
        RenderDriver.OpenGL => "opengl",
        RenderDriver.OpenGLES2 => "opengles2",
        RenderDriver.Vulkan => "vulkan",
        RenderDriver.Gpu => "gpu",
        RenderDriver.Software => "software",
        RenderDriver.Auto => throw new ArgumentOutOfRangeException(nameof(driver), driver, "Auto has no name: Khaos leaves the hint unset."),
        _ => throw new ArgumentOutOfRangeException(nameof(driver), driver, null)
    };

    internal static string NameOf(VideoDriver driver) => driver switch
    {
        VideoDriver.Wayland => "wayland",
        VideoDriver.X11 => "x11",
        VideoDriver.Offscreen => "offscreen",
        VideoDriver.Auto => throw new ArgumentOutOfRangeException(nameof(driver), driver, "Auto has no name: Khaos leaves the hint unset."),
        _ => throw new ArgumentOutOfRangeException(nameof(driver), driver, null)
    };

    internal static void Reset(string name) => SDL3.ThrowIfFailed(SDL3.ResetHint(name));

    internal static bool Set(string name, string value, ILogger logger)
    {
        if (SDL3.GetEnv(name) is { } kept)
        {
            KeptFromEnvironment(logger, name, kept, value);
            return false;
        }

        SDL3.ThrowIfFailed(SDL3.SetHint(name, value));
        return true;
    }

    internal static void ThrowIfRefused(IDictionary<string, string> native)
    {
        foreach (string name in native.Keys)
        {
            if (!name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                throw new KhaosException($"The setting {EngineOptions.Section}:{EngineOptions.NativeSection}:{name} is not a native setting. Their names start with {Prefix}, such as SDL_VIDEO_ALLOW_SCREENSAVER.");

            if (_ownedByOptions.TryGetValue(name, out string? option))
                throw new KhaosException($"The native setting {name} comes from the setting {option}. Use {option} instead.");

            if (_invariants.Contains(name))
                throw new KhaosException($"The native setting {name} is kept by Khaos and cannot be changed.");
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The setting {Setting} asks for {Driver}, which this computer does not support ({Reason}). Khaos picks another one.")]
    internal static partial void DriverMissing(ILogger logger, string setting, string driver, string? reason);

    [LoggerMessage(Level = LogLevel.Debug, Message = "The native setting {Name} keeps {Kept} from the environment, instead of {Value}.")]
    private static partial void KeptFromEnvironment(ILogger logger, string name, string kept, string value);
}
