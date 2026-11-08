// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;
using System.Numerics;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class TransformBenchmarks
{
    private const int Seed = 42;

    private readonly Transform _camera = Transform.Create(new Point(-320f, -180f), Angle.Zero, new Vector2(2f, 2f));

    private Point[] _positions = [];
    private Angle[] _rotations = [];
    private Transform[] _transforms = [];
    private Rect[] _sprites = [];

    [Params(16, 256, 4096)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(Seed);

        _positions = new Point[Count];
        _rotations = new Angle[Count];
        _transforms = new Transform[Count];
        _sprites = new Rect[Count];

        for (int i = 0; i < Count; i++)
        {
            _positions[i] = new Point(random.NextSingle() * 1280f, random.NextSingle() * 720f);
            _rotations[i] = Angle.FromDegrees(random.NextSingle() * 360f);
            _transforms[i] = Transform.Create(_positions[i], _rotations[i], Vector2.One, new Point(16f, 16f));
            _sprites[i] = new Rect(0f, 0f, 32f, 32f);
        }
    }

    [Benchmark]
    public float Compose()
    {
        float sum = 0f;

        foreach (Transform transform in _transforms)
            sum += (_camera * transform).Matrix.M31;

        return sum;
    }

    [Benchmark]
    public float Create()
    {
        float sum = 0f;

        for (int i = 0; i < _positions.Length; i++)
            sum += Transform.Create(_positions[i], _rotations[i], Vector2.One, new Point(16f, 16f)).Matrix.M31;

        return sum;
    }

    [Benchmark]
    public float TransformPoint()
    {
        float sum = 0f;

        foreach (Transform transform in _transforms)
        {
            Point point = transform.TransformPoint(new Point(32f, 0f));
            sum += point.X + point.Y;
        }

        return sum;
    }

    [Benchmark]
    public float TransformRect()
    {
        float sum = 0f;

        for (int i = 0; i < _transforms.Length; i++)
            sum += _transforms[i].TransformRect(_sprites[i]).Area;

        return sum;
    }

    [Benchmark]
    public int TryInvert()
    {
        int count = 0;

        foreach (Transform transform in _transforms)
        {
            if (transform.TryInvert(out _))
                count++;
        }

        return count;
    }
}
