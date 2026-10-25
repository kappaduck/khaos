# Benchmarks

How fast the parts of Khaos that run every frame are: drawing sprites, handling events, reading input, finding
assets. They are measured on every release, so you can see that a new version did not make your game slower.

## How to read the tables

- **Mean** is how long one operation takes, on average. Compare the rows of one table with each other: the numbers
  come from a shared server, not a gaming PC, so they say more about how operations compare than about the speed of
  your own computer.
- **Allocated** is how much memory one operation asks for. Khaos aims for zero in everything that runs every frame:
  memory that is not allocated never has to be cleaned up by .NET's garbage collector, which could otherwise cause a
  small stutter in the middle of your game.

## Measure on your computer

From a clone of the [Khaos repository](https://github.com/kappaduck/khaos):

```bash
dotnet run -c Release --project benchmarks/Khaos.Benchmarks -- --filter '*'
```

It takes a few minutes and prints the same tables, measured on your own hardware.

> [!NOTE]
> The results appear here with each stable release of Khaos. This version of the site has none yet.
