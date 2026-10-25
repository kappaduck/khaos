# KHAOS100: No browser version of Khaos for this .NET version

| Item     | Value                    |
| :------- | :----------------------- |
| Category | Build                    |
| Severity | Error                    |
| Where    | Building the browser app |

## Cause

Your browser app targets a .NET version that your version of Khaos does not support in the browser yet, for
example `net12.0` with a Khaos made for .NET 11.

## Why

In the browser, Khaos's native libraries are built into the game itself, and Khaos ships a separate copy of them for
each .NET version, made with that version's browser tools. A Khaos release supports the .NET versions that existed when it came out. Without
a matching copy, the game cannot be built.

## Reported

```text
error KHAOS100: KappaDuck.Khaos.Native has no SDL for net12.0 (looked in …/wasm/net12.0/). Target a .NET version it supports, or update KappaDuck.Khaos.
```

## Fix

Either:

- target a .NET version Khaos supports: `<TargetFramework>net11.0</TargetFramework>`, in `Directory.Build.props` for a
  game made with the template;
- or update Khaos to a version that supports your .NET version: change `KhaosVersion` in `Directory.Packages.props`.

## Not reported

- Desktop builds: Windows and Linux use the same libraries whatever the .NET version.

## Suppressing

Not possible: without these libraries, the browser version cannot be built.
