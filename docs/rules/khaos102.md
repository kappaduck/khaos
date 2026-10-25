# KHAOS102: Unknown KhaosWebCompression value

| Item     | Value                      |
| :------- | :------------------------- |
| Category | Build                      |
| Severity | Error                      |
| Where    | Publishing the browser app |

## Cause

`KhaosWebCompression` in your browser app's project has a value Khaos does not know. Only `Brotli`, `Gzip` and `None`
are valid.

## Why

This setting decides which compressed copies of your game's files go in the published zip, to make the download
faster. With a typo, the zip would silently be published without them, and your game would load slower with nothing
to tell you why.

## Reported

For `<KhaosWebCompression>brotli-max</KhaosWebCompression>`:

```text
error KHAOS102: KhaosWebCompression is 'brotli-max': use Brotli, Gzip or None.
```

## Fix

Use one of the three values, or remove the line to get the default, `Brotli`. See
[The browser](../guides/browser.md#publishing) for what each one does.

## Not reported

- `dotnet build` and `dotnet run`: the zip is only made by `dotnet publish`.

## Suppressing

Not possible: fix the value.
