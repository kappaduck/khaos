# How a game runs

Every game repeats the same thing many times per second. Look at what the player did, move things, draw the screen.
Each repetition is a **frame**, and the code that repeats is the **game loop**. At 60 frames per second, your game
draws a new picture every 16 milliseconds.

In Khaos you do not write that loop. Khaos runs it for you and calls methods of your game at the right moment of each
frame. You override the ones you need.

> [!NOTE]
> Khaos is young: the names on this page can still change from one version to the next until 1.0.

## The methods of your game

| Method                                    | Khaos calls it                                                                   | Use it to                                                       |
| :---------------------------------------- | :------------------------------------------------------------------------------- | :-------------------------------------------------------------- |
| `Initialize()`                            | Once, before the first frame                                                     | Load your images, sounds and fonts, and set up the game         |
| `OnEvent(in Event e)`                     | For each thing that happened: a key pressed, a mouse click, a gamepad plugged in | React to the player                                             |
| `FixedUpdate(in GameTime time)`           | At a steady rate, 60 times per second by default                                 | Move things, check collisions, apply the rules of your game     |
| `Update(in GameTime time)`                | Once per frame                                                                   | Animate what only looks nice, such as a spinning coin or a menu |
| `Draw(Renderer renderer, in Frame frame)` | Once per frame, last                                                             | Draw the frame                                                  |
| `Shutdown()`                              | Once, after the last frame, while the window is still open                       | Save the player's settings, such as the size of the window      |

Each frame goes in this order: events first, then updates, then drawing. You only override what you need: the
smallest game only has `Draw`. Call `Exit()` from anywhere to close the game.

`Shutdown` runs only when `Initialize` did. If your game implements `IDisposable`, keep `Dispose` for releasing what
.NET holds, such as a file you opened. Khaos also disposes a game that never started, so use `Window` in `Shutdown`,
never in `Dispose`.

## Why two kinds of update

Not every computer draws at the same speed: a gaming PC may draw 144 frames per second, a browser tab on a laptop 30.
If your character moved a little every frame, it would run five times faster on the gaming PC. Khaos avoids that by
splitting the work in two:

- **`FixedUpdate`** is your gameplay. It always runs at the same rate, whatever the screen does: on a fast computer
  some frames skip it, on a slow one it runs twice in a frame to catch up. `time.Delta` is always the same value, so
  a jump is always the same height and a replay always ends the same way.
- **`Update`** runs once per frame, with the real time since the last frame. Use it for what does not change the
  game, only its look: a menu that slides in, a coin that spins.

When in doubt, put it in `FixedUpdate`.

So a faster screen never changes your game, only how smooth it looks: see [Frame rate](#frame-rate).

Each method gets its own clock in `time`:

| Property     | In `FixedUpdate`                      | In `Update`                      |
| :----------- | :------------------------------------ | :------------------------------- |
| `time.Delta` | The length of a step, always the same | The time since the last frame    |
| `time.Total` | The steps so far times `Delta`        | The frame times so far, added up |
| `time.Count` | The number of the step, from 1        | The number of the frame, from 1  |

`Delta` and `Total` are in seconds. In `FixedUpdate`, `Total` is computed from `Count`, so a replay of the same steps
finds exactly the same values.

## Steps per second

`FixedUpdate` runs 60 times per second by default, the usual rate for a 2D game. A racing game or a fast shooter can
ask for more in its [settings](configuration.md):

```json
{
  "loop": {
    "fixedRate": 120,
    "maxFrameTime": 0.25
  }
}
```

| Setting              | What it does                                                        | Default |
| :------------------- | :------------------------------------------------------------------ | :------ |
| `fixedRate`          | How many times per second `FixedUpdate` runs                        | `60`    |
| `maxFrameTime`       | The most time one frame counts, in seconds, from one step up to `1` | `0.25`  |
| `pauseWhenMinimized` | Whether the game stops while its window is minimized                | `true`  |

In code, set them in `Program.cs`:

```csharp
builder.ConfigureLoop(loop => loop.FixedRate = 120);
```

`maxFrameTime` protects the game when a frame comes very late: the debugger stopped on a breakpoint, the browser tab
was hidden. Khaos counts that frame as a quarter of a second at most, so `FixedUpdate` catches up with at most 15
steps, and the game goes on where it was instead of rushing through everything it missed. `Update` gets the same
capped time, so an animation does not jump either.

## When the window is minimized

On the desktop, the game stops while its window is minimized. `FixedUpdate`, `Update` and `Draw` are not called, and
the game uses no processor time. When the window comes back, the game picks up exactly where it was, as if no time
had passed.

`OnEvent` still receives `WindowMinimized` and `WindowRestored`. A game that must keep running, or that wants to
handle it itself (show its own pause menu, lower the music), turns the pause off and reacts to those events:

```csharp
builder.ConfigureLoop(loop => loop.PauseWhenMinimized = false);
```

In the browser, a hidden tab is slowed down by the browser itself, to about one frame per second. With the cap of
`maxFrameTime`, the game then runs at a quarter of its speed until the tab comes back.

## Smooth drawing

Because gameplay and drawing run at different rates, a frame is often drawn between two gameplay steps. `Draw`
receives `frame.Alpha`, a number between 0 and 1 that says where it is: 0 right on the last step, 0.5 halfway to the
next one. Draw moving things between their previous and current positions, and the movement stays smooth even when
the two rates differ:

```csharp
internal sealed class MyGame() : Game2D("Ball", 1280, 720)
{
    private Vector2 _previous = new(640f, 360f);
    private Vector2 _position = new(640f, 360f);
    private Vector2 _velocity = new(240f, 180f);   // pixels per second

    protected override void OnEvent(in Event e)
    {
        if (e is KeyPressed { Code: Scancode.Escape })
            Exit();
    }

    protected override void FixedUpdate(in GameTime time)
    {
        _previous = _position;
        _position += _velocity * time.Delta;
    }

    protected override void Draw(Renderer renderer, in Frame frame)
    {
        // Where the ball is right now, between its last two positions.
        Vector2 drawn = Vector2.Lerp(_previous, _position, frame.Alpha);

        renderer.Clear(Color.Teal);
        // Draw the ball at `drawn`.
    }
}
```

Without it, the game still works; things just move in small jumps when the rates differ.

## Frame rate

Khaos draws one frame each time the screen refreshes, which is called vertical sync (VSync): 60 frames per second
on a 60 Hz screen, 144 on a 144 Hz gaming monitor. More frames make movement smoother; gameplay keeps stepping at its
own steady rate in `FixedUpdate`, so the game itself does not change.

You can change that in your [settings](configuration.md), for example to save a laptop's battery:

```json
{
  "graphics": {
    "vsync": true,
    "maxFrameRate": 60
  }
}
```

| Setting        | What it does                                                                                                                                    | Default |
| :------------- | :---------------------------------------------------------------------------------------------------------------------------------------------- | :------ |
| `vsync`        | Waits for the screen before showing each frame. Turned off, the game draws as fast as it can, which can tear the picture and keep the fans busy | `true`  |
| `maxFrameRate` | Never draws more frames per second than this. `0` means no limit                                                                                | `0`     |

`vsync` takes one of these values:

| Value        | The game shows a frame                                                                          |
| :----------- | :---------------------------------------------------------------------------------------------- |
| `true`       | Each time the screen refreshes                                                                  |
| `false`      | As soon as it is drawn, as fast as it can                                                       |
| `"adaptive"` | Each time the screen refreshes, but right away when it is late, so it does not stutter          |
| `2`, `3`, …  | Every 2 or 3 refreshes: 30 or 20 frames per second on a 60 Hz screen, to save even more battery |

Not every graphics card supports `"adaptive"` or a number; where it does not, the game waits for each refresh, as with
`true`. In code, set them in `Program.cs`:

```csharp
builder.ConfigureGraphics(graphics =>
{
    graphics.VSync = VSync.Adaptive;
    graphics.MaxFrameRate = 60;
});
```

In the browser, `vsync` does nothing: the page draws at the screen's rate, as browsers decide. `maxFrameRate` still
works there, but the browser then times each frame less precisely, so keep it for saving battery.

## Reacting to the player

`OnEvent` receives one event at a time. Check which kind it is with a `switch` or an `is`, as with any C# type:

```csharp
protected override void OnEvent(in Event e)
{
    switch (e)
    {
        case KeyPressed { Code: Scancode.Space }:
            Jump();
            break;

        case KeyPressed { Code: Scancode.Escape }:
            Exit();
            break;
    }
}
```

Events are for things that happen once: a key pressed, a button clicked. For things that last, such as a key held to
walk, the input classes tell you what is pressed right now, from `FixedUpdate`. Events cost nothing to receive where you
can match on them as much as you like, even in a game that runs at a high frame rate.

Your game can also send events of its own, such as "level finished", and receive them in `OnEvent` the same way.
[Events](events.md) lists every event and shows how to send your own.

## Drawing happens in Draw

The `Renderer` you draw with is handed to `Draw`, and only there. You cannot draw from `Update` or from somewhere else
by mistake: keep your drawing code in `Draw`, and everything else outside it.

When `Draw` returns, Khaos shows what you drew on the screen: there is nothing to call at the end. Each frame starts
from nothing, so begin `Draw` with `renderer.Clear`.

## Starting the game with Program.cs

Each app has a `Program.cs` that starts your game:

```csharp
return KhaosApplication.CreateBuilder(args)
                       .Build<MyGame>()
                       .Run();
```

`CreateBuilder` reads your [settings](configuration.md) and gets logging ready. Between that line and the next, you
can add your own services or settings. `Build<MyGame>()` creates your game, and `Run()` opens the window and starts
the loop, until the game exits.

## Keep each frame short

A frame should do its work and return. If one frame waits (a `Thread.Sleep`, a `.Wait()` on a task) or computes for
a long time, the game freezes until it is done. In the browser it is worse: the game runs on the page's only thread,
so the whole tab freezes.

Load what takes time in `Initialize`, before the game starts, and split long work over several frames.
