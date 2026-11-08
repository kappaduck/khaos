// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Mathematics;

namespace KappaDuck.Khaos.Benchmarks.Micro.Mathematics;

public class CellEnumeratorBenchmarks
{
    private RectI _area;

    [ParamsSource(nameof(Grids))]
    public SizeI Grid { get; set; }

    public static IEnumerable<SizeI> Grids => [new(8, 8), new(40, 23), new(256, 256)];

    [GlobalSetup]
    public void Setup() => _area = new RectI(new PointI(3, 5), Grid);

    [Benchmark]
    public long Cells()
    {
        long sum = 0;

        foreach (PointI cell in _area.Cells)
            sum += cell.X + cell.Y;

        return sum;
    }

    [Benchmark(Baseline = true)]
    public long NestedLoops()
    {
        long sum = 0;

        for (int y = _area.Top; y < _area.Bottom; y++)
        {
            for (int x = _area.Left; x < _area.Right; x++)
                sum += x + y;
        }

        return sum;
    }
}
