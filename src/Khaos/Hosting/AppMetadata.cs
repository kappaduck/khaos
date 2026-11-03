// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting;

internal sealed record AppMetadata
{
    public string? Copyright { get; init; }

    public string? Creator { get; init; }

    public string? Identifier { get; init; }

    public required string Name { get; init; }

    public Uri? Url { get; init; }

    public string? Version { get; init; }
}
