// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Actions;

namespace KappaDuck.Khaos.Tools.Site;

public static class GamePublisher
{
    public const string ComingSoonPath = "docs/play/coming-soon.html";
    public const string ExamplesProject = "docs/examples/Web/Examples.Web.csproj";

    public static async Task<bool> PublishAsync()
    {
        foreach (string directory in Directory.GetDirectories("demo"))
        {
            string name = Path.GetFileName(directory);
            string project = Path.Combine(directory, $"{name}.Web", $"{name}.Web.csproj");

            if (!File.Exists(project))
                continue;

            string? output = null;

            if (HasCode(Path.Combine(directory, name)))
            {
                output = $"artifacts/play/demo/{name}";

                if (await Processes.RunAsync("dotnet", "publish", project, "--output", output) != 0)
                    return false;
            }

            if (!Place(output, $"docs/_site/demo/{name}/play"))
                return false;
        }

        foreach (string category in Directory.GetDirectories("docs/examples"))
        {
            foreach (string directory in Directory.GetDirectories(category))
            {
                if (!File.Exists(Path.Combine(directory, "readme.md")))
                    continue;

                string name = Path.GetFileName(directory);
                string example = $"{Path.GetFileName(category)}/{name}";
                string? output = null;

                if (File.Exists(Path.Combine(directory, $"{name}.cs")))
                {
                    output = $"artifacts/play/examples/{example}";

                    if (await Processes.RunAsync("dotnet", "publish", ExamplesProject, $"-p:Example={example}", "--output", output) != 0)
                        return false;
                }

                if (!Place(output, $"docs/_site/examples/{example}/play"))
                    return false;
            }
        }

        return true;
    }

    private static bool HasCode(string folder)
        => Directory.Exists(folder) && Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).Any();

    private static bool Place(string? output, string destination)
    {
        Directory.CreateDirectory(destination);

        if (output is null)
        {
            File.Copy(ComingSoonPath, Path.Combine(destination, "index.html"), true);
            return true;
        }

        string page = Path.Combine(output, "wwwroot");

        if (!File.Exists(Path.Combine(page, "index.html")))
        {
            GitHubActions.Error($"No wwwroot/index.html in {output}: where did dotnet publish put the page?");
            return false;
        }

        Folders.Copy(page, destination, overwrite: true);
        return true;
    }
}
