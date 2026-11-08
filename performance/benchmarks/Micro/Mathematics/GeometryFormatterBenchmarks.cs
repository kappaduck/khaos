// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;
using System.Globalization;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class GeometryFormatterBenchmarks
{
    private readonly char[] _chars = new char[64];
    private readonly byte[] _bytes = new byte[64];
    private readonly Rect _rect = new(12.5f, 48.25f, 1280f, 720f);

    private CultureInfo _provider = CultureInfo.InvariantCulture;

    [Params("", "fr-CA")]
    public string Culture { get; set; } = "";

    [GlobalSetup]
    public void Setup() => _provider = CultureInfo.GetCultureInfo(Culture);

    [Benchmark(Baseline = true)]
    public string ToStringAllocates() => _rect.ToString(null, _provider);

    [Benchmark]
    public int TryFormat()
    {
        _rect.TryFormat(_chars, out int written, default, _provider);
        return written;
    }

    [Benchmark]
    public int TryFormatUtf8()
    {
        _rect.TryFormat(_bytes, out int written, default, _provider);
        return written;
    }

    [Benchmark]
    public int TryFormatGroupedDigits()
    {
        _rect.TryFormat(_chars, out int written, "N0", _provider);
        return written;
    }
}
