// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;
using System.Globalization;
using System.Text;

namespace KappaDuck.Khaos.Tools.Native;

public static class SDLUpdater
{
    private const string NuspecPath = "native/packages/Native.nuspec";
    private const string PackageReadmePath = "native/packages/readme.md";
    private const string ReadmePath = "readme.md";
    private const string ChangelogPath = "changelog.md";

    public const string VersionsPath = "native/sdl.env";

    public static IReadOnlyList<string> Files { get; } = [VersionsPath, ReadmePath, ChangelogPath, NuspecPath, PackageReadmePath];

    public static string PullRequestBody(SDLUpdateResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        StringBuilder builder = new();

        builder.Append(Table(result.Updates, "Newer stable SDL releases are available. This updates the versions in `native/sdl.env`, which `native.yml` builds for the desktop and for WebAssembly once this is merged.", withLinks: true));
        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"`KappaDuck.Khaos.Native` is bumped from `{result.PreviousNative}` to `{result.NextNative}`, the SDL tables in `readme.md` and `native/packages/readme.md` are updated, and so is its line in the [Unreleased] section of `changelog.md`.");
        builder.AppendLine();
        builder.AppendLine("> [!IMPORTANT]");
        builder.AppendLine("> The bump level follows the SDL version change. If a patch release adds native API, change the bump to a minor version before merging.");

        return builder.ToString();
    }

    public static string StepSummary(SDLUpdateResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Table(result.Updates, "### SDL updates", withLinks: false);
    }

    public static async Task<SDLUpdateResult> UpdateAsync(string root, Func<SDLLibrary, Task<Version?>> latestStable)
    {
        ArgumentNullException.ThrowIfNull(latestStable);

        SDLVersions versions = await SDLVersions.ParseAsync(Path.Combine(root, VersionsPath));
        NativePackage package = await NativePackage.ParseAsync(Path.Combine(root, NuspecPath));
        BundledVersionsTable bundledTable = await BundledVersionsTable.ParseAsync(Path.Combine(root, PackageReadmePath));
        CompatibilityTable compatibilityTable = await CompatibilityTable.ParseAsync(Path.Combine(root, ReadmePath));
        Changelog changelog = await Changelog.ParseAsync(Path.Combine(root, ChangelogPath));

        Version previousNative = package.Version;

        List<SDLUpdate> updates = [];
        Dictionary<string, Version> bundled = [];

        foreach (SDLLibrary library in SDLLibrary.All)
        {
            Version current = versions.GetVersion(library.Key);
            bundled[library.DisplayName] = current;

            Version? latest = await latestStable(library);

            if (latest is null)
            {
                await Console.Out.WriteLineAsync($"{library.Key}: no stable release found on libsdl-org/{library.Repository}.");
                continue;
            }

            if (latest <= current)
            {
                await Console.Out.WriteLineAsync($"{library.Key}: up to date ({current}).");
                continue;
            }

            await Console.Out.WriteLineAsync($"{library.Key}: {current} -> {latest}");

            versions.SetVersion(library.Key, latest);

            bundled[library.DisplayName] = latest;
            updates.Add(new SDLUpdate(library, current, latest));
        }

        if (updates.Count == 0)
            return new SDLUpdateResult(updates, previousNative, previousNative);

        package.Bump(updates);
        bundledTable.Update(bundled);

        Apply(compatibilityTable, changelog, package.Version, bundled);

        await versions.SaveAsync();
        await package.SaveAsync();
        await bundledTable.SaveAsync();
        await compatibilityTable.SaveAsync();
        await changelog.SaveAsync();

        return new SDLUpdateResult(updates, previousNative, package.Version);
    }

    internal static void Apply(CompatibilityTable table, Changelog changelog, Version native, IReadOnlyDictionary<string, Version> bundled)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(changelog);

        table.SetSource(native, bundled);
        changelog.SetNative(native, [.. table.ChangedSinceRelease()]);
    }

    private static string Table(IReadOnlyList<SDLUpdate> updates, string heading, bool withLinks)
    {
        StringBuilder builder = new();

        builder.AppendLine(heading);
        builder.AppendLine();
        builder.AppendLine("| Library | Current | Latest |");
        builder.AppendLine("| :---: | :---: | :---: |");

        foreach (SDLUpdate update in updates)
        {
            string latest = withLinks
                ? $"[`{update.Latest}`](https://github.com/libsdl-org/{update.Library.Repository}/releases/tag/release-{update.Latest})"
                : update.Latest.ToString();

            builder.AppendLine(CultureInfo.InvariantCulture, $"| `{update.Library.Key}` | `{update.Current}` | {latest} |");
        }

        return builder.ToString();
    }
}
