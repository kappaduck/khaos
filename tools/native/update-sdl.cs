#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Native;

if (args.Length > 0)
    return Script.Usage("dotnet tools/native/update-sdl.cs");

if (!Script.RunsFromRoot([.. SDLUpdater.Files]))
    return Script.Failed;

return await Script.RunAsync(static async () =>
{
    using HttpClient http = new() { Timeout = TimeSpan.FromSeconds(30) };
    string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

    SDLUpdateResult result = await SDLUpdater.UpdateAsync(Environment.CurrentDirectory, library => SDLReleases.LatestStableAsync(http, library, token));

    if (!result.Updated)
    {
        Console.WriteLine("All SDL libraries are up to date.");
        GitHubActions.SetOutput("updated", "false");

        return 0;
    }

    Console.WriteLine($"Native: {result.PreviousNative} -> {result.NextNative}");

    await File.WriteAllTextAsync("pr-body.md", SDLUpdater.PullRequestBody(result));

    GitHubActions.SetOutput("updated", "true");
    GitHubActions.AppendStepSummary(SDLUpdater.StepSummary(result));

    Console.WriteLine($"Updated {result.Updates.Count} SDL {(result.Updates.Count == 1 ? "library" : "libraries")} in '{SDLUpdater.VersionsPath}'.");
    return 0;
});
