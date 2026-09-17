// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tests;

/// <summary>
/// Chooses the video driver the whole test session runs against.
/// </summary>
/// <remarks>
/// Headless by default, so the suite runs on a machine or an agent without a display.
/// <list type="bullet">
///   <item><description><c>dotnet test</c> runs headless.</description></item>
///   <item><description><c>dotnet test -- --test-parameter video-driver=x11</c> uses a named driver.</description></item>
///   <item><description><c>dotnet test -- --test-parameter video-driver=default</c> lets the platform choose, which is what a real application does.</description></item>
/// </list>
/// An <c>SDL_VIDEODRIVER</c> environment variable that is already set always wins.
/// </remarks>
internal static class VideoDriverSetup
{
    private const string Default = "default";
    private const string Headless = "dummy";
    private const string Hint = "SDL_VIDEODRIVER";
    private const string Parameter = "video-driver";

    [Before(TestSession)]
    public static void Configure()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Hint)))
            return;

        string driver = TestContext.Configuration.Get(Parameter) ?? Headless;

        if (string.Equals(driver, Default, StringComparison.OrdinalIgnoreCase))
            return;

        Environment.SetEnvironmentVariable(Hint, driver);
    }
}
