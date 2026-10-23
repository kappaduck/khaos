# Benchmarks

BenchmarkDotNet measurements of the hot paths: batching in the renderer, event dispatch, input mapping,
asset lookups. Allocation counts matter as much as time: the game loop is expected to allocate nothing.

```bash
dotnet run -c Release --project benchmarks/Khaos.Benchmarks -- --filter '*'
```

On GitHub, `.github/workflows/benchmarks.yml` runs them:

- on demand (Actions > Benchmarks > Run workflow), on any branch and with any filter: the tables appear on
  the run's summary page and the reports are kept as the `benchmarks` artifact;
- on every stable release, called by the documentation workflow: the tables are added to
  `docs/benchmarks/index.md` and published with the documentation of that version.
