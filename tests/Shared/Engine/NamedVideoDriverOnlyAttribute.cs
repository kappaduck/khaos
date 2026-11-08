// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Testing.Engine;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class NamedVideoDriverOnlyAttribute() : SkipAttribute("The platform chooses the video driver in this session.")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
        => Task.FromResult(VideoDriverSetup.Driver is null);
}
