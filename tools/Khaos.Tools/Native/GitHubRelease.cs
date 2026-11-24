// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace KappaDuck.Khaos.Tools.Native;

internal sealed record GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string? Tag { get; init; }

    public bool Draft { get; init; }

    public bool Prerelease { get; init; }
}
