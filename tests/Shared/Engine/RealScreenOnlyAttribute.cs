// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Testing.Engine;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RealScreenOnlyAttribute() : SkipAttribute("The offscreen video driver does not support this window operation.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(string.Equals(VideoDriverSetup.Driver, NativeNames.OffscreenVideoDriver, StringComparison.OrdinalIgnoreCase));
}
