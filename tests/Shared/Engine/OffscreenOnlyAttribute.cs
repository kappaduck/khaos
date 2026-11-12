// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Testing.Engine;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class OffscreenOnlyAttribute() : SkipAttribute("The test relies on the offscreen video driver.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(!string.Equals(VideoDriverSetup.Driver, NativeNames.OffscreenVideoDriver, StringComparison.OrdinalIgnoreCase));
}
