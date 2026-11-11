# KHAOS004: [CustomEvents] group is named like a member of CustomEvent

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Warning        |
| Where    | Your game code |

## Cause

A `[CustomEvents]` group has the name of a public member of `CustomEvent`, such as `Is`, `TryGet`, `Equals` or
`ToString`.

## Why

`OnEvent` reads a group through a property of `CustomEvent` named after the group:
`case CustomEvent { LevelEvents: LevelFinished done }`. When `CustomEvent` already has a member with that name, C#
picks that member, and the events of the group never match.

## Reported

```csharp
public readonly record struct LevelFinished(int Level);

[CustomEvents(typeof(LevelFinished))]
public readonly partial struct TryGet;
//                             ~~~~~~ KHAOS004: 'TryGet' has the name of a member of CustomEvent, so
//                                    'CustomEvent { TryGet: … }' reads that member instead of the group;
//                                    rename the group
```

## Fix

Give the group a name that says what its events are about, such as `LevelEvents`.

## Suppressing

Only if you never match the group in a pattern and read it with `LevelEvents.From(in custom)` instead.
