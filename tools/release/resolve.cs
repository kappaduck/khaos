#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Releases;

if (args is not [string packageId, string version, string channelName, .. string[] rest] || rest.Length > 1)
    return Script.Usage("dotnet tools/release/resolve-version.cs <package-id> <version> <channel> [<tag>]");

string tag = rest is [string given] ? given : string.Empty;
ReleaseChannel channel = TagRelease.ChannelOf(channelName);

return await Script.RunAsync(async () =>
{
    string[]? published = null;

    if (channel == ReleaseChannel.Beta)
    {
        using HttpClient http = new();
        published = await NuGetVersions.FetchAsync(http, packageId);

        Console.WriteLine(published is null
            ? "Package not found on NuGet, starting at beta.1"
            : $"NuGet has {published.Length} versions of {packageId}");
    }

    VersionResolution resolution = TagRelease.Resolve(version, tag, channel, published);

    return resolution switch
    {
        ResolvedVersion resolved => Resolved(resolved),
        string error => Refused(error)
    };

    static int Resolved(ResolvedVersion resolved)
    {
        Console.WriteLine($"Resolved version: {resolved.Version}");
        GitHubActions.SetOutput("Version", resolved.Version);
        GitHubActions.SetOutput("Suffix", resolved.Suffix ?? string.Empty);

        return 0;
    }

    static int Refused(string error)
    {
        GitHubActions.Error(error);
        return Script.Failed;
    }
});
