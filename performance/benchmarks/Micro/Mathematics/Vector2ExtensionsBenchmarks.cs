// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;
using System.Numerics;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class Vector2ExtensionsBenchmarks
{
    private const int Seed = 42;

    private readonly Vector2 _target = new(640f, 360f);
    private readonly Angle _turn = Angle.FromDegrees(3f);

    private Vector2[] _positions = [];
    private Vector2[] _velocities = [];

    [Params(16, 256, 4096)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(Seed);

        _positions = new Vector2[Count];
        _velocities = new Vector2[Count];

        for (int i = 0; i < Count; i++)
        {
            _positions[i] = new Vector2(random.NextSingle() * 1280f, random.NextSingle() * 720f);
            _velocities[i] = i % 16 == 0
                ? Vector2.Zero
                : new Vector2((random.NextSingle() * 1000f) - 500f, (random.NextSingle() * 1000f) - 500f);
        }
    }

    [Benchmark]
    public Vector2 ClampLength()
    {
        Vector2 sum = Vector2.Zero;

        foreach (Vector2 velocity in _velocities)
            sum += velocity.ClampLength(300f);

        return sum;
    }

    [Benchmark]
    public Vector2 MoveTowards()
    {
        Vector2 sum = Vector2.Zero;

        foreach (Vector2 position in _positions)
            sum += position.MoveTowards(_target, 4f);

        return sum;
    }

    [Benchmark]
    public Vector2 NormalizeOrZero()
    {
        Vector2 sum = Vector2.Zero;

        foreach (Vector2 velocity in _velocities)
            sum += velocity.NormalizeOrZero();

        return sum;
    }

    [Benchmark]
    public Vector2 Rotate()
    {
        Vector2 sum = Vector2.Zero;

        foreach (Vector2 velocity in _velocities)
            sum += velocity.Rotate(_turn);

        return sum;
    }
}
