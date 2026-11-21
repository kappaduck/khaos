#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Releases;

if (args is not [])
    return Script.Usage("dotnet tools/release/prepare.cs");

if (!Script.RunsFromRoot([.. ReleasePreparation.Files]))
    return Script.Failed;

return await Script.RunAsync(static async () =>
{
    DateOnly today = DateOnly.FromDateTime(DateTime.Now);
    PreparedRelease release = await ReleasePreparation.PrepareAsync(Environment.CurrentDirectory, today);

    Console.WriteLine($"Prepared Khaos {release.Version} ({today:yyyy-MM-dd}):");
    Console.WriteLine($"  {ReleasePreparation.ChangelogPath}: dated the [Unreleased] section");
    Console.WriteLine($"  {ReleasePreparation.ReadmePath}: added Khaos {release.Version} with the native packages {release.Native} to the SDL compatibility table");

    foreach (AnalyzerProject analyzer in release.Analyzers)
    {
        Console.WriteLine(analyzer.ShippedRules.Count == 0
            ? $"  {analyzer.Name}: no unshipped rules"
            : $"  {analyzer.Name}: shipped {string.Join(", ", analyzer.ShippedRules)} rules under Release {release.Version}");
    }

    return 0;
}, "No file was modified.");
