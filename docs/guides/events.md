# Events

An event is something that happened once: a key pressed, the window resized, the player closing the game, or
something your own game announces, such as a level finished. Khaos hands each event to your game's `OnEvent`, one at
a time, before the frame updates and draws.

> [!NOTE]
> Khaos is young: the names on this page can still change from one version to the next until 1.0.

## Receiving events

Override `OnEvent` and check which kind of event it is with a `switch`, as with any C# type. A property pattern
checks what the event holds in the same line:

```csharp
protected override void OnEvent(in Event e)
{
    switch (e)
    {
        case KeyPressed { Code: Scancode.Space }:
            Jump();
            break;

        case WindowResized resized:
            FitCamera(resized.Size);
            break;

        default:
            break;
    }
}
```

For a single check, `is` reads well too: `if (e is KeyPressed { Code: Scancode.Escape }) Exit();`.

Prefer a `switch` statement with a `default` branch to a `switch` expression that lists every kind. Each version of
Khaos adds kinds of events (the mouse and gamepads come next), and the compiler then warns about a `switch` expression
that no longer covers them all.

## The events Khaos sends

| Event               | Khaos sends it when                                            | What it holds                                    |
| :------------------ | :------------------------------------------------------------- | :----------------------------------------------- |
| `QuitRequested`     | The player closes the window, or presses Ctrl+C in the console | Nothing                                          |
| `WindowResized`     | The window changes size                                        | `Size`, the new size inside it, in logical units |
| `WindowFocusGained` | The window becomes the one the keyboard types into             | Nothing                                          |
| `WindowFocusLost`   | Another window takes the keyboard                              | Nothing                                          |
| `WindowMinimized`   | The window is minimized                                        | Nothing                                          |
| `WindowRestored`    | The window comes back from minimized                           | Nothing                                          |
| `KeyPressed`        | A key goes down, once per press                                | `Code`, the key that went down                   |
| `KeyReleased`       | A key goes up                                                  | `Code`, the key that went up                     |

They all live in `KappaDuck.Khaos.Events`, and `Scancode` lives in `KappaDuck.Khaos.Input`.

### Closing the game

`QuitRequested` reaches `OnEvent` first, so you can save the player's progress, and Khaos closes the game once
`OnEvent` returns. Khaos does nothing on Escape, because many games open a pause menu with it. Decide what it does in
your own `OnEvent`:

```csharp
case KeyPressed { Code: Scancode.Escape }:
    Exit();
    break;
```

### Keys

`Scancode` names a key by its place on the keyboard, not by the letter printed on it. A game that moves with
`Scancode.W`, `Scancode.A`, `Scancode.S` and `Scancode.D` keeps those four keys in the same place on every keyboard. On
a French AZERTY keyboard, `Scancode.A` is the key labeled Q.

`KeyPressed` comes once per press. Holding a key does not repeat it, so `case KeyPressed { Code: Scancode.Space }:
Jump();` jumps once however long the key stays down. For something that lasts as long as the key is held, such as
walking, remember it between the two events:

```csharp
case KeyPressed { Code: Scancode.Left }:
    _walkingLeft = true;
    break;

case KeyReleased { Code: Scancode.Left }:
    _walkingLeft = false;
    break;
```

### The window

`WindowResized` gives the size in logical units, the same units as the size you give your game, so a window of
1280 × 720 stays 1280 × 720 on a screen scaled to 200%. A game often pauses when the player looks away:

```csharp
case WindowFocusLost or WindowMinimized:
    _paused = true;
    break;

case WindowFocusGained or WindowRestored:
    _paused = false;
    break;
```

## Sending your own events

Your game can send events of its own, and receive them in `OnEvent` like the ones Khaos sends. The code that notices
something (the player reached the exit) does not need to know the code that reacts to it (the score, the music, the
next level).

Declare each event as a `readonly record struct`, then send it with `EventQueue.TryPush`, from anywhere in your game
and from any thread:

```csharp
public readonly record struct LevelFinished(int Level, TimeSpan Time);

EventQueue.TryPush(new LevelFinished(3, elapsed));
```

It reaches `OnEvent` as a `CustomEvent`, on the game's thread, before the next frame draws. Read it with `TryGet`:

```csharp
case CustomEvent custom when custom.TryGet(out LevelFinished finished):
    ShowScore(finished.Time);
    break;
```

### Groups of events

To match your events by name, list them in a group, a `readonly partial struct` marked `[CustomEvents]`:

```csharp
public readonly record struct LevelFinished(int Level, TimeSpan Time);
public readonly record struct CoinCollected(int Value);

[CustomEvents(typeof(LevelFinished), typeof(CoinCollected))]
public readonly partial struct LevelEvents;
```

Khaos writes the rest of the group as you type, so `CustomEvent` gets a `LevelEvents` property:

```csharp
case CustomEvent { LevelEvents: LevelFinished finished }:
    ShowScore(finished.Time);
    break;

case CustomEvent { LevelEvents: CoinCollected { Value: > 10 } }:
    PlayBigCoinSound();
    break;
```

Code in the namespace of the group, or in one inside it, uses the group with no `using`. From another namespace, add
the group's namespace with a `using`, as for any type. When a group is declared wrong, your editor shows an error
right away, with a page that explains it: [KHAOS001](../rules/khaos001.md) to [KHAOS005](../rules/khaos005.md).

### What an event can hold

An event holds values (numbers, `bool`, `TimeSpan`, vectors, your own structs of values) and strings. It is copied
when you send it and again when `OnEvent` reads it, so nothing can change it on the way. To send a list, send one
event per item.

Read an event inside `OnEvent`. Once `OnEvent` returns, its place is used by the next event, and a `CustomEvent` you
kept reads nothing anymore: `TryGet` returns `false`. Copy the values you need into your game instead.

### How many can wait

Up to 16 events of each type can wait to be handled at once. When all 16 are waiting, `TryPush` returns `false` and
the event is not sent. It also returns `false` when the game is not running. To let more of one type wait, register
it before its first push:

```csharp
EventQueue.Register<CoinCollected>(capacity: 256);
```

### Do not send an event for each event you receive

Khaos hands your game every waiting event before it draws, including the ones sent during `OnEvent`. An `OnEvent`
that sends a new event each time it receives one never lets the frame draw, and the game freezes. Send the next event
from your update code, or stop the chain after a while.

## What events cost

Receiving an event, matching it with patterns and sending your own allocate nothing, and tests check it on every
change. A frame that handles 64 events takes about 10 microseconds more than an empty one on a desktop computer, less
than a thousandth of a frame at 60 frames per second. Match on events as much as you like.

## What comes next

The mouse and gamepads arrive with Khaos 0.2.0, as events in the same `OnEvent`, along with input actions and a way
to ask which keys are held right now (see the [roadmap](roadmap.md)). Two examples show this page at work:
[Keys](../examples/events/keys/readme.md) and [Game events](../examples/events/game-events/readme.md).
