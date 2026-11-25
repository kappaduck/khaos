// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace KappaDuck.Khaos.Tools.Benchmarks;

[JsonSerializable(typeof(BenchmarkReport))]
internal sealed partial class BenchmarkJsonContext : JsonSerializerContext;
