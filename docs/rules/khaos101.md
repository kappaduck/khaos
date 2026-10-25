# KHAOS101: The browser workload and Khaos do not match

| Item     | Value                    |
| :------- | :----------------------- |
| Category | Build                    |
| Severity | Error                    |
| Where    | Building the browser app |

## Cause

The `wasm-tools` workload installed on your computer and your version of Khaos were made for different releases of
.NET 11. It usually happens after you update one and not the other.

## Why

To build the browser version, the workload combines your game, .NET and Khaos's native libraries into one file. Those libraries
are shipped by Khaos,
already prepared with the tools of one .NET release; the workload brings the tools of the release you have installed.
When the two differ, the result would fail to build with a long, cryptic error. Khaos checks before that happens and
tells you what to update.

## Reported

```text
error KHAOS101: SDL for net11.0 was compiled with Emscripten 6.0.3, but the wasm-tools workload links with Emscripten 6.0.4. Update
KappaDuck.Khaos or run 'dotnet workload update'. Set KhaosSkipEmscriptenCheck to true to try anyway.
```

Emscripten is the tool that turns Khaos's native libraries, written in C, into WebAssembly. "SDL" in the message is
the main one.

## Fix

Update the side that is behind:

- **Your workload is older**: run `dotnet workload update`.
- **Your Khaos is older**: update `KhaosVersion` in `Directory.Packages.props` to the latest version.

If you are not sure, do both.

## Not reported

- When the workload's tools cannot be found at all: .NET reports that itself.

## Suppressing

To try the build anyway, for example when only the last digit of the two versions differs, add to the browser app's
project:

```xml
<PropertyGroup>
  <KhaosSkipEmscriptenCheck>true</KhaosSkipEmscriptenCheck>
</PropertyGroup>
```

If the build then fails or the game misbehaves in the browser, remove it and update instead.
