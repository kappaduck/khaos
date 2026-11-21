// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Actions;
using System.Globalization;

namespace KappaDuck.Khaos.Tools.Packages;

internal static class LocalFeed
{
    private const string TemplatePackage = "KappaDuck.Khaos.Templates";

    public const string PackagesFilter = "packages.slnf";

    public static async Task<PackedTemplate?> PackAsync(string feed, string suffix)
    {
        int exitCode = await Processes.RunAsync("dotnet", "pack", PackagesFilter, "--configuration", "Release", "--output", feed, $"-p:VersionSuffix={suffix}");

        if (exitCode != 0)
            return null;

        string? template = Directory.EnumerateFiles(feed, $"{TemplatePackage}.*-{suffix}.nupkg").FirstOrDefault();

        if (template is null)
        {
            GitHubActions.Error($"'{TemplatePackage}' was not packed into '{feed}'.");
            return null;
        }

        return new PackedTemplate(template, Path.GetFileNameWithoutExtension(template)[(TemplatePackage.Length + 1)..]);
    }

    public static string SuffixOf(string label) => string.Create(CultureInfo.InvariantCulture, $"{label}.{DateTime.UtcNow:yyyyMMddHHmmss}");

    internal static string NuGetConfig(string feed) => $"""
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <packageSources>
            <clear />
            <add key="khaos-local" value="{feed}" />
            <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
          </packageSources>
        </configuration>
        """;
}
