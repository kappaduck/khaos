#!/usr/bin/env -S dotnet --

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Releases;

if (args.Length > 1)
    return Script.Usage("dotnet tools/release/next.cs [x.y.z]   (the next minor without a version)");

if (!Script.RunsFromRoot(ReleasePreparation.ProjectPath, ReleasePreparation.ChangelogPath))
    return Script.Failed;

string? requested = args is [string version] ? version : null;

return await Script.RunAsync(async () =>
{
    NextVersion version = await ReleasePreparation.OpenNextAsync(Environment.CurrentDirectory, requested, DateTime.Now.Year);

    Console.WriteLine($"Prepared the next version: {version.Current} -> {version.Next}");
    Console.WriteLine($"  {ReleasePreparation.ProjectPath}: VersionPrefix is {version.Next}");
    Console.WriteLine($"  {ReleasePreparation.ChangelogPath}: opened the [Unreleased] section");
    return 0;
}, "No file was modified.");
