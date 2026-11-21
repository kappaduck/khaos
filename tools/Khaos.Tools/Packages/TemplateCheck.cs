// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Actions;
using System.Text.RegularExpressions;

namespace KappaDuck.Khaos.Tools.Packages;

public static partial class TemplateCheck
{
    private const string GameName = "DuckQuest";

    public static IReadOnlyList<string> Platforms { get; } = ["both", "desktop", "browser"];

    public static IReadOnlyList<string> Readmes { get; } = ["readme.md", "assets/nuget/readme.md"];

    public static async Task<int> RunAsync(IReadOnlyList<string> platforms)
    {
        ArgumentNullException.ThrowIfNull(platforms);

        string suffix = LocalFeed.SuffixOf("ci");

        string work = Path.Combine(Path.GetTempPath(), "khaos-template-check", suffix);
        string feed = Path.Combine(work, "feed");
        string hive = Path.Combine(work, "hive");

        Dictionary<string, string> packages = new() { ["NUGET_PACKAGES"] = Path.Combine(work, "packages") };

        Console.WriteLine($"Working in {work}");

        PackedTemplate? template = await LocalFeed.PackAsync(feed, suffix);

        if (template is null)
            return Script.Failed;

        int exitCode = await Processes.RunAsync("dotnet", "new", "install", template.Path, "--debug:custom-hive", hive);

        if (exitCode != 0)
            return exitCode;

        foreach (string platform in platforms)
        {
            string game = Path.Combine(work, platform, GameName);

            exitCode = await CreateGameAsync(GameName, game, platform, template.Version, hive, feed);

            if (exitCode != 0)
                return exitCode;

            exitCode = await Processes.RunAsync(packages, "dotnet", "build", game);

            if (exitCode != 0)
            {
                GitHubActions.Error($"The '{platform}' game of the template does not build.");
                return exitCode;
            }

            if (!HasNoHelloWorld(game))
            {
                GitHubActions.Error($"The '{platform}' game still prints \"Hello, Khaos!\" instead of running a game.");
                return Script.Failed;
            }
        }

        foreach (string readme in Readmes)
        {
            IReadOnlyList<UsageSample>? samples = UsageSamples.Parse(await File.ReadAllTextAsync(readme));

            if (samples is null)
            {
                GitHubActions.Error($"Each csharp block under '## Usage' in '{readme}' starts with the path it belongs to (// Pong/PongGame.cs).");
                return Script.Failed;
            }

            string name = samples[0].Path.Split('/')[0];
            string game = Path.Combine(work, "readme", readme.Replace('/', '-'), name);

            exitCode = await CreateGameAsync(name, game, "both", template.Version, hive, feed);

            if (exitCode != 0)
                return exitCode;

            foreach (UsageSample sample in samples)
                await File.WriteAllTextAsync(Path.Combine(game, sample.Path), sample.Code);

            exitCode = await Processes.RunAsync(packages, "dotnet", "build", Path.Combine(game, $"{name}.Desktop"));

            if (exitCode != 0)
            {
                GitHubActions.Error($"The sample under '## Usage' in '{readme}' does not build.");
                return exitCode;
            }
        }

        Console.WriteLine($"The template {template.Version} creates and builds: {string.Join(", ", platforms)}, and the samples of {string.Join(", ", Readmes)}.");
        return 0;
    }

    private static async Task<int> CreateGameAsync(string name, string game, string platform, string version, string hive, string feed)
    {
        int exitCode = await Processes.RunAsync("dotnet", "new", "khaos", "--name", name, "--output", game, "--platform", platform, "--khaos-version", version, "--debug:custom-hive", hive);

        if (exitCode == 0)
            await File.WriteAllTextAsync(Path.Combine(game, "nuget.config"), LocalFeed.NuGetConfig(feed));

        return exitCode;
    }

    private static bool HasNoHelloWorld(string game)
        => Directory.EnumerateFiles(game, "Program.cs", SearchOption.AllDirectories).All(static file => !HelloWorld.IsMatch(File.ReadAllText(file)));

    [GeneratedRegex("Hello, Khaos!", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex HelloWorld { get; }
}
