// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting;

internal sealed record AppMetadata
{
    public required string Name { get; init; }

    public string? Version { get; init; }

    public string? Identifier { get; init; }

    public string? Creator { get; init; }

    public string? Copyright { get; init; }

    public Uri? Url { get; init; }
}
