# Performance

How fast Khaos is, measured on every stable release, so you can check that a new version did not make your game
slower.

## What is measured

- **A whole game**: a game someone recorded, replayed by a demo for thousands of frames, in the three builds you can
  publish (desktop, native desktop, browser). It shows how long the first frame takes, how steady the frames are, and
  whether the game ever pauses for memory. It comes later, once the demos exist.
- **One frame**: the engine runs a frame without a screen, with nothing in it. This is what a frame costs before your
  game adds its own work.
- **One operation at a time**: a rectangle test, a color parsed from text, a title written to the window. Each table
  shows what a call costs and that it allocates nothing.

## How to read the tables

- **Mean** is how long one operation takes, on average. Compare the rows of one table with each other: the numbers
  come from a shared server, not a gaming PC, so they say more about how operations compare than about the speed of
  your own computer.
- **Allocated** is how much memory one operation asks for. Khaos aims for zero in everything that runs every frame:
  memory that is not allocated never has to be cleaned up by .NET's garbage collector, which could otherwise cause a
  small stutter in the middle of your game. A `-` means the same as `0 B`.
- **Runtime** says how the game was compiled. `.NET 11.0` is the usual way, where the code is compiled while the game
  runs. `NativeAOT 11.0` is compiled ahead of time into a native program, which is how a published desktop game
  ships. The two rarely differ once the game is running; where they do, the table shows it.
- **Ratio** compares a row with the first row of its group, so `0.50` means twice as fast.
- **Compared with** lists what changed since the previous version: both versions are measured on the same server, one
  after the other, so this table can be trusted across versions. A time that changed by less than 5% is left out.

## Measure on your computer

From a clone of the [Khaos repository](https://github.com/kappaduck/khaos):

```bash
dotnet run -c Release --project performance/benchmarks -- --filter '*'
```

It takes a few minutes and prints the same tables, measured on your own hardware. To measure one frame only, use
`--filter '*.Frames.*'`. To compare the two ways of compiling, add `--runtimes net11.0 nativeaot11.0`; it takes
longer, as the native build has to be compiled first.

> [!NOTE]
> The results appear here with each stable release of Khaos. This version of the site has none yet.
