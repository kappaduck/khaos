// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Testing.Engine;

internal static class VideoDriverSetup
{
    private const string Default = "default";
    private const string Parameter = "video-driver";

    internal static string? Driver { get; } = ReadDriver();

    [Before(TestSession)]
    public static void Setup()
    {
        if (Driver is null)
        {
            SDLEnvironment.Unset(NativeNames.VideoDriver);
            return;
        }

        SDLEnvironment.Set(NativeNames.VideoDriver, Driver);
    }

    private static string? ReadDriver()
    {
        string driver = TestContext.Parameters.TryGetValue(Parameter, out List<string>? values) && values.Count > 0
            ? values[^1]
            : NativeNames.OffscreenVideoDriver;

        return string.Equals(driver, Default, StringComparison.OrdinalIgnoreCase) ? null : driver;
    }
}
