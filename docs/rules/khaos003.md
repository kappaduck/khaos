# KHAOS003: event in a [CustomEvents] group has the wrong shape

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Error          |
| Where    | Your game code |

## Cause

A type listed in `[CustomEvents]` is not a valid event:

- it is not a `readonly record struct`, or it is generic;
- it is less accessible than the group, such as an `internal` event in a `public` group;
- it holds an object other than a `string`, such as a list or a class;
- it is named `Cases`, a name the group uses for its own members.

## Why

An event is copied when you send it with `EventQueue.TryPush` and again when `OnEvent` reads it, so it has to be a
small value that nothing can change on the way. The group takes and returns the event in its public members, so the
event has to be visible wherever the group is. An object inside an event would be shared between the code that sent
it and the code that reads it.

## Reported

```csharp
public readonly record struct HighScores(List<int> Scores);

[CustomEvents(typeof(HighScores))]
//            ~~~~~~~~~~~~~~~~~~ KHAOS003: 'HighScores' in 'ScoreEvents' holds 'Scores' of type
//                               System.Collections.Generic.List<int>; an event holds only values and strings
public readonly partial struct ScoreEvents;
```

## Fix

Send one event per value, and collect them where you handle them:

```csharp
public readonly record struct HighScore(int Rank, int Score);

[CustomEvents(typeof(HighScore))]
public readonly partial struct ScoreEvents;
```

## Suppressing

Not possible: fix the event.
