// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Releases;

public readonly record struct LatestRelease(bool IsLatest, string? Previous, Version? Highest);
