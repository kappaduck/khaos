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

## Code errors

Errors in your game's code, shown in your editor as you type. Khaos cannot write the code your game relies on until
they are fixed.

| Code | What it means |
| :--- | :------------ |
| [`KHAOS001`](khaos001.md) | `[CustomEvents]` is not on a `readonly partial struct` |
| [`KHAOS002`](khaos002.md) | A `[CustomEvents]` group has no events |
| [`KHAOS003`](khaos003.md) | An event in a `[CustomEvents]` group has the wrong shape |
| [`KHAOS005`](khaos005.md) | An event in a `[CustomEvents]` group is listed twice or shares a name |
| [`KHAOS007`](khaos007.md) | `[InputActions]` is on more than one enum |
| [`KHAOS008`](khaos008.md) | The `[InputActions]` enum has more than 64 actions |
| [`KHAOS009`](khaos009.md) | An input action has no kind or two kinds |
| [`KHAOS011`](khaos011.md) | A `[CustomEvents]` group has more than 255 events |

## Code warnings

Warnings about your game's code, shown in your editor as you type: something that builds but does not do what it
seems to.

| Code | What it means |
| :--- | :------------ |
| [`KHAOS004`](khaos004.md) | A `[CustomEvents]` group has the name of a member of `CustomEvent` |
| [`KHAOS006`](khaos006.md) | A game takes its window as a constructor parameter |
| [`KHAOS010`](khaos010.md) | An input action has the name of a member of `InputActions` |

## Turning a warning off

A warning can be silenced for one line with `#pragma warning disable KHAOS004` (with its code), or for the whole
project in your `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.KHAOS004.severity = none
```

Each page says when silencing makes sense. Errors cannot be turned off: the game cannot be built without fixing
them.

> [!NOTE]
> Codes that start with `KHI` are rules for the source code of Khaos itself. You never see them in a game.
