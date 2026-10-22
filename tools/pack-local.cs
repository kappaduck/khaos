#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics;
using System.Globalization;

const string solution = "khaos.slnx";
const string feed = "artifacts/local-feed";
const string templatePackage = "KappaDuck.Khaos.Templates";

if (!File.Exists(solution))
{
    Console.Error.WriteLine("error: Run the script from the repository root.");
    return 1;
}

string suffix = string.Create(CultureInfo.InvariantCulture, $"local.{DateTime.UtcNow:yyyyMMddHHmmss}");

int exitCode = await RunAsync("dotnet", "restore", solution, $"-p:VersionSuffix={suffix}");

if (exitCode != 0)
    return exitCode;

exitCode = await RunAsync("dotnet", "pack", solution, "--no-restore", "--configuration", "Release", "--output", feed, $"-p:VersionSuffix={suffix}");

if (exitCode != 0)
    return exitCode;

string? template = Directory.EnumerateFiles(feed, $"{templatePackage}.*-{suffix}.nupkg").FirstOrDefault();

if (template is null)
{
    Console.Error.WriteLine($"error: '{templatePackage}' was not packed into '{feed}'.");
    return 1;
}

exitCode = await RunAsync("dotnet", "new", "install", template, "--force");

if (exitCode != 0)
    return exitCode;

string khaosVersion = Path.GetFileNameWithoutExtension(template)[(templatePackage.Length + 1)..];
string source = Path.GetFullPath(feed);

Console.WriteLine();
Console.WriteLine($"Packed {khaosVersion} into {source}");
Console.WriteLine("Create a game from it:");
Console.WriteLine($"  dotnet new khaos -o MyGame --khaos-version {khaosVersion}");
Console.WriteLine("  dotnet new nugetconfig -o MyGame");
Console.WriteLine($"  dotnet nuget add source \"{source}\" --name khaos-local --configfile MyGame/nuget.config");
return 0;

static async Task<int> RunAsync(string fileName, params string[] arguments)
{
    Console.WriteLine($"> {fileName} {string.Join(' ', arguments)}");

    ProcessStartInfo info = new(fileName, arguments) { UseShellExecute = false };

    using Process process = Process.Start(info) ?? throw new InvalidOperationException($"Cannot start '{fileName}'.");
    await process.WaitForExitAsync();

    return process.ExitCode;
}
