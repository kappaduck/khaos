// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Graphics;
using System.Globalization;

namespace KappaDuck.Khaos.Benchmarks.Micro.Graphics;

public class ColorBenchmarks
{
    private readonly char[] _chars = new char[32];
    private readonly Color _from = Color.CornflowerBlue;
    private readonly Color _to = Color.Crimson;

    [Params("#DC143C", "#DC143C80", "#ABC")]
    public string Hex { get; set; } = "";

    [Benchmark(Baseline = true)]
    public Color FromHex() => Color.FromHex(Hex);

    [Benchmark]
    public bool TryFromHex() => Color.TryFromHex(Hex, out _);

    [Benchmark]
    public Color Lerp() => Color.Lerp(_from, _to, 0.35f);

    [Benchmark]
    public int TryFormat()
    {
        Color.Crimson.TryFormat(_chars, out int written, default, CultureInfo.InvariantCulture);
        return written;
    }
}
