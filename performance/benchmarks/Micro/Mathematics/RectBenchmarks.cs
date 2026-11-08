// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class RectBenchmarks
{
    private const int Seed = 42;

    private readonly Rect _viewport = new(0f, 0f, 1280f, 720f);

    private Rect[] _sprites = [];
    private Point[] _points = [];
    private Point[] _inside = [];
    private Point[] _outside = [];
    private IEnumerable<Point> _insideSequence = [];

    [Params(16, 256, 4096)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(Seed);

        _sprites = new Rect[Count];
        _points = new Point[Count];
        _inside = new Point[Count];
        _outside = new Point[Count];

        for (int i = 0; i < Count; i++)
        {
            _sprites[i] = new Rect((random.NextSingle() * 1600f) - 160f, (random.NextSingle() * 900f) - 90f, 32f, 32f);
            _points[i] = new Point((random.NextSingle() * 1600f) - 160f, (random.NextSingle() * 900f) - 90f);
            _inside[i] = new Point(random.NextSingle() * 1280f, random.NextSingle() * 720f);
            _outside[i] = new Point(1300f + (random.NextSingle() * 300f), random.NextSingle() * 720f);
        }

        _insideSequence = _inside;
    }

    [Benchmark]
    public int Contains()
    {
        int count = 0;

        foreach (Point point in _points)
            count += _viewport.Contains(point) ? 1 : 0;

        return count;
    }

    [Benchmark]
    public bool ContainsAll() => _viewport.ContainsAll(_inside.AsSpan());

    [Benchmark]
    public bool ContainsAllFromASequence() => _viewport.ContainsAll(_insideSequence);

    [Benchmark]
    public bool ContainsAny() => _viewport.ContainsAny(_outside.AsSpan());

    [Benchmark]
    public int ContainsRect()
    {
        int count = 0;

        foreach (Rect sprite in _sprites)
            count += _viewport.Contains(sprite) ? 1 : 0;

        return count;
    }

    [Benchmark]
    public int Intersects()
    {
        int count = 0;

        foreach (Rect sprite in _sprites)
            count += _viewport.Intersects(sprite) ? 1 : 0;

        return count;
    }

    [Benchmark]
    public long RoundOut()
    {
        long area = 0;

        foreach (Rect sprite in _sprites)
            area += sprite.RoundOut().Area;

        return area;
    }

    [Benchmark]
    public Rect TryEnclose()
    {
        Rect.TryEnclose(_points.AsSpan(), out Rect bounds);
        return bounds;
    }

    [Benchmark]
    public float TryIntersect()
    {
        float area = 0f;

        foreach (Rect sprite in _sprites)
        {
            if (Rect.TryIntersect(_viewport, sprite, out Rect intersection))
                area += intersection.Area;
        }

        return area;
    }

    [Benchmark]
    public Rect Union()
    {
        Rect bounds = _sprites[0];

        for (int i = 1; i < _sprites.Length; i++)
            bounds = Rect.Union(bounds, _sprites[i]);

        return bounds;
    }
}
