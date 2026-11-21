// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;
using System.Text.RegularExpressions;

namespace KappaDuck.Khaos.Tools.Releases;

public static partial class TagRelease
{
    private const string Beta = "beta";

    public static ReleaseChannel ChannelOf(string? name)
        => string.Equals(name, Beta, StringComparison.OrdinalIgnoreCase) ? ReleaseChannel.Beta : ReleaseChannel.Release;

    public static LatestRelease Latest(string tag, ReadOnlySpan<string> tags)
    {
        string version = Normalize(tag);
        List<Version> stable = [];

        foreach (string candidate in tags)
        {
            string name = Normalize(candidate);

            if (StableVersion.IsMatch(name))
                stable.Add(Version.Parse(name));
        }

        stable.Sort();

        Version? highest = stable.Count > 0 ? stable[^1] : null;
        Version? current = StableVersion.IsMatch(version) ? Version.Parse(version) : null;
        Version? previous = current is null ? null : stable.FindLast(c => c < current);

        bool latest = string.Equals(version, highest?.ToString(), StringComparison.Ordinal);

        return new LatestRelease(latest, previous is null ? null : $"v{previous}", highest);
    }

    public static string LineOf(string tag)
    {
        string version = Normalize(tag);
        int lastDot = version.LastIndexOf('.', StringComparison.Ordinal);

        return lastDot < 0 ? version : version[..lastDot];
    }

    public static bool Matches(string tag, string prefix, ReleaseChannel channel)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        string version = Normalize(tag);

        if (channel is not ReleaseChannel.Beta)
            return string.Equals(version, prefix, StringComparison.Ordinal);

        return version.StartsWith($"{prefix}-", StringComparison.Ordinal) && BetaSuffix.IsMatch(version.AsSpan(prefix.Length + 1));
    }

    public static string Normalize(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        return tag.StartsWith('v', StringComparison.OrdinalIgnoreCase) ? tag[1..] : tag;
    }

    public static VersionResolution Resolve(string version, string? tag, ReleaseChannel channel, params ReadOnlySpan<string> published)
    {
        if (channel is not ReleaseChannel.Beta)
            return new ResolvedVersion(version);

        Regex betaVersion = new($"^{Regex.Escape(version)}-beta\\.([0-9]+)$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        long latest = 0;

        foreach (string candidate in published)
        {
            Match match = betaVersion.Match(candidate);

            if (match.Success)
                latest = Math.Max(latest, Number(match));
        }

        if (string.IsNullOrEmpty(tag))
            return BetaOf(version, latest + 1);

        Match requested = betaVersion.Match(Normalize(tag));

        if (!requested.Success)
            return $"The tag {tag} is not a beta of {version}. Tag the pre-release v{version}-beta.{latest + 1}.";

        long number = Number(requested);

        if (number <= latest)
            return $"The tag asks for {version}-beta.{number}, but NuGet already has {version}-beta.{latest}. Tag the pre-release v{version}-beta.{latest + 1}.";

        return BetaOf(version, number);

        static long Number(Match match) => long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

        static ResolvedVersion BetaOf(string version, long number)
        {
            string suffix = string.Create(CultureInfo.InvariantCulture, $"{Beta}.{number}");
            return new ResolvedVersion($"{version}-{suffix}", suffix);
        }
    }

    [GeneratedRegex(@"^beta\.[0-9]+$", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex BetaSuffix { get; }

    [GeneratedRegex(@"^[0-9]+\.[0-9]+\.[0-9]+$", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex StableVersion { get; }
}
