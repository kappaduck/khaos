// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Testing.Engine;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class MultipleScreensOnlyAttribute() : SkipAttribute("The test needs a second screen.")
{
    private const string Offscreen = "offscreen";

    private static readonly Lazy<bool> _hasASecondScreen = new(CountScreens);

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        bool offscreen = string.Equals(VideoDriverSetup.Driver, Offscreen, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(offscreen || !_hasASecondScreen.Value);
    }

    private static bool CountScreens()
    {
        using EngineHost host = EngineHost.Start(new AppMetadata { Name = "Duck Quest" });
        return Display.All.Count > 1;
    }
}
