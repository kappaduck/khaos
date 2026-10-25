# KHAOS103: The wasm-tools workload is not installed

| Item     | Value                    |
| :------- | :----------------------- |
| Category | Build                    |
| Severity | Error                    |
| Where    | Building the browser app |

## Cause

You build a browser app, and the `wasm-tools` workload is not installed on your computer, or not for the .NET
version your game targets.

## Why

The browser version of your game is one WebAssembly file with your game, .NET and Khaos's native libraries inside,
and only the `wasm-tools` workload can put those libraries in it. Without the workload, .NET would still build the app,
without them, and only
mention it in an easy-to-miss warning: the page would load, then fail as soon as the game starts. Khaos stops the
build instead, with the command that fixes it.

## Reported

```text
error KHAOS103: The wasm-tools workload for net11.0 is not installed, so SDL cannot be linked into dotnet.native.wasm. Run 'dotnet workload install wasm-tools'.
```

## Fix

```bash
dotnet workload install wasm-tools
```

Updating the .NET SDK does not install workloads, so this can come back after an SDK update: run
`dotnet workload update` then. If your project sets `WasmNativeWorkload` to `false`, remove that line.

## Not reported

- Desktop builds, which do not need the workload.
- When .NET reports the missing workload first, as `NETSDK1147`: the fix is the same.

## Suppressing

Not possible: without the workload, the game cannot run in the browser.
