// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Native;

public sealed record SDLUpdateResult(IReadOnlyList<SDLUpdate> Updates, Version PreviousNative, Version NextNative)
{
    public bool Updated => Updates.Count > 0;
}
