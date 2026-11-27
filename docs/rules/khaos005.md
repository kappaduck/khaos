# KHAOS005: event in a [CustomEvents] group is listed twice or shares a name

| Item     | Value          |
| :------- | :------------- |
| Category | Events         |
| Severity | Error          |
| Where    | Your game code |

## Cause

The same event is listed twice in `[CustomEvents]`, or two events of the group have the same name in two
namespaces, such as `Menu.Started` and `Game.Started`.

## Why

The group holds each event under the name of the event, with one constructor per event. Two entries for one event,
or two events with one name, would give the group two members with the same name.

## Reported

```csharp
namespace Arcade
{
    [CustomEvents(typeof(Menu.Started), typeof(Game.Started))]
    //                                  ~~~~~~~~~~~~~~~~~~~~ KHAOS005: 'Arcade.Game.Started' in 'ArcadeEvents'
    //                                                       has the same name as 'Arcade.Menu.Started';
    //                                                       rename one of them
    public readonly partial struct ArcadeEvents;
}
```

## Fix

List each event once, and give each event of a group its own name, such as `MenuStarted` and `GameStarted`. Two
events that must keep their name can go in two groups.

## Suppressing

Not possible: fix the list.
