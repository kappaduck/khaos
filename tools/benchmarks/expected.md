## Compared with 0.1.0

Both versions measured on the same runner, one after the other. Time changes under 5% are left out.

| Benchmark                                                              |   0.1.0 |   0.2.0 | Ratio | Time   | Allocated  |
| :--------------------------------------------------------------------- | ------: | ------: | ----: | :----- | :--------- |
| `Frame.FrameBenchmarks.FrameWithEvents(Count: 64)`                     | 1.20 μs | 1.60 μs |  1.33 | Slower | 0 B → 48 B |
| `Micro.Mathematics.GeometryFormatterBenchmarks.TryFormat(Culture: "")` |  336 ns |  336 ns |  1.00 | Same   | 32 B → 0 B |
| `Micro.Mathematics.RectBenchmarks.Union`                               | 3.10 ns | 2.20 ns |  0.71 | Faster | 0 B        |

> [!WARNING]
> The allocations went up in 1 benchmark.

New in 0.2.0: `Micro.Input.KeyboardBenchmarks.IsDown`.

Gone since 0.1.0: `Micro.Graphics.ColorBenchmarks.FromHex`.
