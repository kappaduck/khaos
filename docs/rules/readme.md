# Errors and warnings

When something in your game or its project would not work, Khaos tells you in the build output, with a code that
starts with `KHAOS`. Each code has a page here that explains what happened and how to fix it.

## Build errors

Reported while your game builds, before anything runs. They all concern the browser version.

| Code | What it means |
| :--- | :------------ |
| [`KHAOS100`](khaos100.md) | Khaos has no browser version for your game's .NET version |
| [`KHAOS101`](khaos101.md) | The `wasm-tools` workload and Khaos were made for different .NET releases |
| [`KHAOS102`](khaos102.md) | `KhaosWebCompression` has a value Khaos does not know |
| [`KHAOS103`](khaos103.md) | The `wasm-tools` workload is not installed |

## Code warnings

Warnings about your game's code, shown in your editor as you type: an asset name that does not exist, an asset
loaded at the wrong moment. They arrive with the features they check.

## Turning a warning off

A warning can be silenced for one line with `#pragma warning disable KHAOS001` (with its code), or for the whole
project in your `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.KHAOS001.severity = none
```

Each page says when silencing makes sense. Build errors cannot be turned off: the game cannot be built without
fixing them.

> [!NOTE]
> Codes that start with `KHI` are rules for the source code of Khaos itself. You never see them in a game.
