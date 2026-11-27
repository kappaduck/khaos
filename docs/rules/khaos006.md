# KHAOS006: [CustomEvents] group has more than 255 events

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Error          |
| Where    | Your game code |

## Cause

The `[CustomEvents]` attribute of a group lists more than 255 events.

## Why

A group keeps which of its events it holds in one byte, and 0 means it holds none. That leaves room for 255 events.

## Reported

```csharp
[CustomEvents(typeof(LevelStarted), typeof(LevelFinished), /* ... 254 more */)]
public readonly partial struct GameEvents;
//                             ~~~~~~~~~~ KHAOS006: 'GameEvents' holds 256 events; a group holds at most 255,
//                                        so split it in two
```

## Fix

Split the events in two groups that each hold the events that belong together, such as `LevelEvents` and
`ShopEvents`. `OnEvent` reads each group by its own name.

## Suppressing

Not possible: fix the declaration.
