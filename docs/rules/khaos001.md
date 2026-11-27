# KHAOS001: [CustomEvents] is not on a readonly partial struct

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Error          |
| Where    | Your game code |

## Cause

`[CustomEvents]` is on a type that is not a `readonly partial struct` at the top level of a namespace: a class, a
record, a struct without `partial` or `readonly`, a generic struct, a struct nested in another type, or a `file`
struct.

## Why

A group lets `OnEvent` match your own events by name. Khaos writes the code that does it inside your struct, which
is why it has to be `partial`, and reads the struct like any event, which is why it has to be `readonly`. That code
lives in a file of its own, out of reach of a `file` struct.

## Reported

```csharp
public readonly record struct LevelFinished(int Level);

[CustomEvents(typeof(LevelFinished))]
public readonly struct LevelEvents;
//                     ~~~~~~~~~~~ KHAOS001: 'LevelEvents' is marked [CustomEvents]; declare it as a
//                                 readonly partial struct, not generic and not nested in another type
```

## Fix

```csharp
public readonly record struct LevelFinished(int Level);

[CustomEvents(typeof(LevelFinished))]
public readonly partial struct LevelEvents;
```

## Suppressing

Not possible: fix the declaration.
