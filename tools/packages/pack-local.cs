#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Packages;

const string Feed = "artifacts/local-feed";

if (args.Length > 0)
    return Script.Usage("dotnet tools/packages/pack-local.cs");

if (!Script.RunsFromRoot(LocalFeed.PackagesFilter))
    return Script.Failed;

return await Script.RunAsync(static async () =>
{
    PackedTemplate? template = await LocalFeed.PackAsync(Feed, LocalFeed.SuffixOf("local"));

    if (template is null)
        return Script.Failed;

    int exitCode = await Processes.RunAsync("dotnet", "new", "install", template.Path, "--force");

    if (exitCode != 0)
        return exitCode;

    string source = Path.GetFullPath(Feed);

    Console.WriteLine();
    Console.WriteLine($"Packed {template.Version} into {source}");
    Console.WriteLine("Create a game from it:");
    Console.WriteLine($"  dotnet new khaos -o MyGame --khaos-version {template.Version}");
    Console.WriteLine("  dotnet new nugetconfig -o MyGame");
    Console.WriteLine($"  dotnet nuget add source \"{source}\" --name khaos-local --configfile MyGame/nuget.config");
    return 0;
});
