// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tests.Helpers;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealScreenOnlyAttribute() : SkipAttribute("The offscreen video driver does not support this window operation.")
{
    private const string Offscreen = "offscreen";

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(string.Equals(VideoDriverSetup.Driver, Offscreen, StringComparison.OrdinalIgnoreCase));
}
