#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Benchmarks;

if (args.Length > 0)
    return Script.Usage("dotnet tools/benchmarks/add-results.cs");

if (!Script.RunsFromRoot(ResultsPage.PagePath))
    return Script.Failed;

return await Script.RunAsync(static async () =>
{
    if (!await ResultsPage.UpdateAsync(Environment.CurrentDirectory))
        GitHubActions.Warning("No benchmark results to publish.");

    return 0;
});
