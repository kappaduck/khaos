// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class PointBenchmarks
{
    private const int Seed = 42;

    private readonly Point _player = new(640f, 360f);

    private Point[] _from = [];
    private Point[] _to = [];

    [Params(16, 256, 4096)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(Seed);

        _from = new Point[Count];
        _to = new Point[Count];

        for (int i = 0; i < Count; i++)
        {
            _from[i] = new Point(random.NextSingle() * 1280f, random.NextSingle() * 720f);
            _to[i] = new Point(random.NextSingle() * 1280f, random.NextSingle() * 720f);
        }
    }

    [Benchmark]
    public float Lerp()
    {
        float sum = 0f;

        for (int i = 0; i < _from.Length; i++)
        {
            Point point = Point.Lerp(_from[i], _to[i], 0.25f);
            sum += point.X + point.Y;
        }

        return sum;
    }

    [Benchmark(Baseline = true)]
    public int NearestByDistance()
    {
        int nearest = -1;
        float best = float.MaxValue;

        for (int i = 0; i < _from.Length; i++)
        {
            float distance = Point.Distance(_player, _from[i]);

            if (distance < best)
            {
                best = distance;
                nearest = i;
            }
        }

        return nearest;
    }

    [Benchmark]
    public int NearestByDistanceSquared()
    {
        int nearest = -1;
        float best = float.MaxValue;

        for (int i = 0; i < _from.Length; i++)
        {
            float distance = Point.DistanceSquared(_player, _from[i]);

            if (distance < best)
            {
                best = distance;
                nearest = i;
            }
        }

        return nearest;
    }

    [Benchmark]
    public long Round()
    {
        long sum = 0;

        foreach (Point point in _from)
        {
            PointI rounded = point.Round();
            sum += rounded.X + rounded.Y;
        }

        return sum;
    }
}
