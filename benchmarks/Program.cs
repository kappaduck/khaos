// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Running;
using KappaDuck.Khaos.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(KhaosConfiguration).Assembly).Run(args, new KhaosConfiguration());
