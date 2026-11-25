#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Benchmarks;

if (!ComparisonOptions.TryParse(args, out ComparisonOptions? options))
    return Script.Usage("dotnet tools/benchmarks/compare.cs --base <folder or file> --diff <folder or file> [--base-name <name>] [--diff-name <name>] [--threshold 5%] [--strict]");

return await Script.RunAsync(() =>
{
    int allocationsUp = BenchmarkComparison.Write(Console.Out, options);
    return Task.FromResult(options.Strict && allocationsUp > 0 ? Script.Failed : 0);
});
