# Performance

How fast Khaos runs, and what it allocates. Allocations matter as much as time. The game loop should allocate
nothing, and a measurement proves it.

```bash
dotnet run -c Release --project performance/benchmarks -- --filter '*'
```

## Three kinds of measurement

Each kind answers its own question. A measurement belongs to exactly one of them.

| Kind         | Measures                                    | It answers                                             | Lives in                           |
| :----------- | :------------------------------------------ | :----------------------------------------------------- | :--------------------------------- |
| **Micro**    | One method                                  | How fast is it, and does it allocate?                  | `benchmarks/Micro/<Namespace>/`    |
| **Frame**    | One whole frame, headless                   | What does a frame cost before the game adds anything?  | `benchmarks/Frame/`                |
| **Scenario** | A recorded game, replayed from start to end | How does a whole game behave over thousands of frames? | One project per demo, here (later) |

**Micro** benchmarks cover the hot paths, the code a game runs every frame. On a hot path, the expected result is
`Allocated = 0 B` (shown as `-`).

**Frame** benchmarks start the engine without a screen, with the offscreen video driver. They measure one frame, in
nanoseconds and bytes. The empty frame is the baseline every feature is compared against.

Micro and Frame are BenchmarkDotNet benchmarks, in `benchmarks/`. To run one kind only, filter on its folder:

```bash
dotnet run -c Release --project performance/benchmarks -- --filter '*.Frame.*'
```

A **Scenario** replays a game someone recorded in a demo, headless and with VSync off, so every run plays exactly
the same game. Each run writes one JSON file with the time to the first frame, the frame times (p50, p95, p99, max),
the allocations and the GC pauses. The same scenario runs as JIT, NativeAOT and in the browser,
and a tool compares the three in one table.

## Allocation tests

Benchmarks report numbers but never fail. `tests/Khaos.Allocations.Tests` does. Each test measures one call with
`GC.GetAllocatedBytesForCurrentThread()` and fails when it allocates more than its budget, usually zero. The tests
run in milliseconds on every pull request, and on the native build too, since the project publishes with
`PublishAot`.

Add an allocation test with every hot path, and a benchmark here when its time matters too.

## JIT and NativeAOT

BenchmarkDotNet runs the same benchmarks on both runtimes in one go:

```bash
dotnet run -c Release --project performance/benchmarks -- --filter '*' --runtimes net11.0 nativeaot11.0
```

The first runtime listed is the baseline of the `Ratio` column. BenchmarkDotNet builds the native executable itself
and runs it as a child process. `PublishAot` in `Benchmarks.csproj` only turns on the NativeAOT warnings at build
time. NativeAOT needs its [prerequisites](https://learn.microsoft.com/dotnet/core/deploying/native-aot/#prerequisites)
installed, and the `DisassemblyDiagnoser` does not work with it.

## Writing a benchmark

- One class per subject, named `<Subject>Benchmarks`. A micro benchmark goes in `Micro/<Namespace>/`, the namespace
  it measures; a frame benchmark goes in `Frame/`.
- Build the inputs in `[GlobalSetup]`, with a fixed seed. The benchmark method only does the work being measured.
- Return the result, so the JIT cannot remove the call.
- Use `[Params]` for the sizes that matter to a game, such as a handful of items, a screenful or a level.
- To compare two ways of doing the same thing, mark one `[Benchmark(Baseline = true)]`.
- A headless benchmark forces the offscreen video driver before the engine starts, and stops the engine in
  `[GlobalCleanup]`.
- Keep a benchmark's full name (namespace, class, method, parameters). It is how two versions are compared, and
  renaming one loses its history.

## Comparing two versions

Two runs on two machines cannot be compared, because a shared runner is too noisy. To know whether a version is
slower or faster than another, both are measured on the same machine, one after the other, and
`tools/compare-benchmarks.cs` compares the benchmarks they share. Time uses the same Mann–Whitney test as
BenchmarkDotNet's `--statisticalTest`, and a change under 5% counts as `Same`. Allocations are exact, so any
increase is reported.

On GitHub, run *Benchmarks* with a `base` (a tag such as `v0.1.0`, or `main` for a pull request). The base runs with
its own benchmark code and its own SDK, so a changed API does not matter. The comparison shows on the run's summary,
and the run fails when an allocation went up (unless `strict` is off).

On your computer, which is quieter than a runner, to confirm a doubtful `Slower`:

```bash
git worktree add ../khaos-base v0.1.0
dotnet run -c Release --project ../khaos-base/performance/benchmarks -- --filter '*.Frame.*'
dotnet run -c Release --project performance/benchmarks -- --filter '*.Frame.*'
dotnet tools/compare-benchmarks.cs --base ../khaos-base/artifacts/benchmarks/results --diff artifacts/benchmarks/results --base-name 0.1.0 --diff-name local
```

### Testing the comparison tool

`tools/benchmarks/` holds fake data, two small reports in BenchmarkDotNet's format with invented numbers,
and the output they must give. They test the tool, not Khaos, so nothing reads them when the benchmarks run. Real
measurements would not work there, because they change a little on every run and a benchmark near the threshold
would flip between `Same` and `Slower`. Each invented case forces one answer instead.

| File          | What it holds                                                                                                                             |
| :------------ | :---------------------------------------------------------------------------------------------------------------------------------------- |
| `base.json`   | Five benchmarks, as measured in a made-up 0.1.0                                                                                           |
| `diff.json`   | The same benchmarks in a made-up 0.2.0. One slower and allocating more, one faster, one allocating less, one unchanged, one gone, one new |
| `expected.md` | The comparison the tool must print for these two files                                                                                    |

After a change to the tool, run its self-test from the root of the repository. It compares the fake data, checks
that the output matches `expected.md` (whatever the line endings) and that `--strict` fails on the extra allocation.
Then check that the tool still reads a real report, where a folder compared with itself shows no change. Both
commands work the same in bash and PowerShell.

```shell
dotnet tools/compare-benchmarks.cs --self-test   # Self-test passed, exit code 0
dotnet tools/compare-benchmarks.cs --base artifacts/benchmarks/results --diff artifacts/benchmarks/results --strict   # No change, exit code 0
```

The fake data changes only with the tool. To test a new case, add a benchmark to both files and write its expected
line in `expected.md` first. It never follows Khaos, so a new benchmark or a new version of Khaos leaves it as it is.

## Results

`Configuration` (in `Program.cs`) writes the reports to `artifacts/benchmarks/results`, as GitHub markdown and compressed JSON.
The memory diagnoser is on, so every table has an `Allocated` column. On GitHub, `.github/workflows/benchmarks.yml`
runs them:

- On demand (Actions > Benchmarks > Run workflow), with any branch, filter and runtimes. The tables show on the
  run's summary, and the reports are kept as the `benchmarks` artifact.
- On every stable release. The tables go into `docs/performance/index.md`, grouped by kind, after a comparison with
  the previous stable release, and are published with the documentation of that version. A regression there is
  shown but does not block, since the release is already on NuGet.
