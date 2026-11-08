// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class RectIBenchmarks
{
    private const int Seed = 42;

    private readonly RectI _viewport = new(0, 0, 1280, 720);

    private RectI[] _sprites = [];
    private PointI[] _points = [];
    private PointI[] _inside = [];
    private PointI[] _outside = [];

    [Params(16, 256, 4096)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(Seed);

        _sprites = new RectI[Count];
        _points = new PointI[Count];
        _inside = new PointI[Count];
        _outside = new PointI[Count];

        for (int i = 0; i < Count; i++)
        {
            _sprites[i] = new RectI(random.Next(-160, 1440), random.Next(-90, 810), 32, 32);
            _points[i] = new PointI(random.Next(-160, 1440), random.Next(-90, 810));
            _inside[i] = new PointI(random.Next(0, 1280), random.Next(0, 720));
            _outside[i] = new PointI(random.Next(1300, 1600), random.Next(0, 720));
        }
    }

    [Benchmark]
    public int Contains()
    {
        int count = 0;

        foreach (PointI point in _points)
            count += _viewport.Contains(point) ? 1 : 0;

        return count;
    }

    [Benchmark]
    public bool ContainsAll() => _viewport.ContainsAll(_inside.AsSpan());

    [Benchmark]
    public bool ContainsAny() => _viewport.ContainsAny(_outside.AsSpan());

    [Benchmark]
    public int ContainsRect()
    {
        int count = 0;

        foreach (RectI sprite in _sprites)
            count += _viewport.Contains(sprite) ? 1 : 0;

        return count;
    }

    [Benchmark]
    public int Intersects()
    {
        int count = 0;

        foreach (RectI sprite in _sprites)
            count += _viewport.Intersects(sprite) ? 1 : 0;

        return count;
    }

    [Benchmark]
    public RectI TryEnclose()
    {
        RectI.TryEnclose(_points.AsSpan(), out RectI bounds);
        return bounds;
    }

    [Benchmark]
    public long TryIntersect()
    {
        long area = 0;

        foreach (RectI sprite in _sprites)
        {
            if (RectI.TryIntersect(_viewport, sprite, out RectI intersection))
                area += intersection.Area;
        }

        return area;
    }

    [Benchmark]
    public RectI Union()
    {
        RectI bounds = _sprites[0];

        for (int i = 1; i < _sprites.Length; i++)
            bounds = RectI.Union(bounds, _sprites[i]);

        return bounds;
    }
}
