#!/usr/bin/env -S dotnet --

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Releases;

string tag = Environment.GetEnvironmentVariable("Tag") ?? string.Empty;

return args switch
{
    ["tag"] => CheckTag(tag, Environment.GetEnvironmentVariable("Prefix") ?? string.Empty, TagRelease.ChannelOf(Environment.GetEnvironmentVariable("Channel"))),
    ["branch"] => await CheckBranchAsync(tag, Environment.GetEnvironmentVariable("GITHUB_SHA") ?? string.Empty),
    ["latest"] => await CheckLatestAsync(tag),
    _ => Script.Usage("dotnet tools/release/check.cs tag|branch|latest (reads Tag, Prefix, Channel and GITHUB_SHA from the environment)")
};

static int CheckTag(string tag, string prefix, ReleaseChannel channel)
{
    if (TagRelease.Matches(tag, prefix, channel))
        return 0;

    GitHubActions.Error(channel == ReleaseChannel.Beta
        ? $"The pre-release tag is {tag}, but a beta of {prefix} is tagged v{prefix}-beta.N, and v{prefix} stays for the release. Fix the tag, or clear Set as a pre-release."
        : $"The release tag is {tag}, but Directory.Build.props says {prefix}, so the release is tagged v{prefix}. A beta is a pre-release. Run tools/release/prepare.cs or fix the tag.");

    return Script.Failed;
}

static async Task<int> CheckBranchAsync(string tag, string sha)
{
    string line = TagRelease.LineOf(tag);

    if ((await Processes.GitAsync("merge-base", "--is-ancestor", sha, "origin/main")).ExitCode == 0)
    {
        Console.WriteLine("The release commit is on main.");
        return 0;
    }

    ProcessOutput revParse = await Processes.GitAsync("rev-parse", "--verify", "--quiet", $"origin/release/{line}");
    ProcessOutput mergeBase = await Processes.GitAsync("merge-base", "--is-ancestor", sha, $"origin/release/{line}");

    if (revParse.ExitCode == 0 && mergeBase.ExitCode == 0)
    {
        Console.WriteLine($"The release commit is on release/{line}.");
        return 0;
    }

    GitHubActions.Error($"The release commit {sha} is on neither main nor release/{line}. Create the release from main, or from release/{line} for a patch, after the release PR is merged.");
    return Script.Failed;
}

static async Task<int> CheckLatestAsync(string tag)
{
    ProcessOutput tags = await Processes.GitAsync("tag", "--list");
    LatestRelease release = TagRelease.Latest(tag, tags.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    GitHubActions.SetOutput("previous", release.Previous ?? string.Empty);

    if (!release.IsLatest)
    {
        GitHubActions.Notice($"{TagRelease.Normalize(tag)} is older than {release.Highest}, so the documentation site stays on {release.Highest}.");
    }

    GitHubActions.SetOutput("latest", release.IsLatest ? "true" : "false");
    return 0;
}
