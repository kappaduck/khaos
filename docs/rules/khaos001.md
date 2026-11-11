# KHAOS001: [CustomEvents] is not on a readonly partial struct

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Error          |
| Where    | Your game code |

## Cause

`[CustomEvents]` is on a type that is not a `readonly partial struct` at the top level of a namespace: a class, a
record, a struct without `partial` or `readonly`, a generic struct, or a struct nested in another type. The same
code is reported when a group lists more than 255 events.

## Why

A group lets `OnEvent` match your own events by name. Khaos writes the code that does it inside your struct, which
is why it has to be `partial`, and reads the struct like any event, which is why it has to be `readonly`.

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

A group with more than 255 events: split it in two groups, such as `LevelEvents` and `ShopEvents`.

## Suppressing

Not possible: fix the declaration.
