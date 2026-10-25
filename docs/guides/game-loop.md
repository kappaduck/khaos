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
| `Load()`                                  | Once, before the first frame                                                     | Load your images, sounds and fonts, and set up the game         |
| `OnEvent(in Event e)`                     | For each thing that happened: a key pressed, a mouse click, a gamepad plugged in | React to the player                                             |
| `FixedUpdate(in GameTime time)`           | At a steady rate, for example 60 times per second                                | Move things, check collisions, apply the rules of your game     |
| `Update(in GameTime time)`                | Once per frame                                                                   | Animate what only looks nice, such as a spinning coin or a menu |
| `Draw(Renderer renderer, in Frame frame)` | Once per frame, last                                                             | Draw the frame                                                  |

Each frame goes in this order: events first, then updates, then drawing. You only override what you need: the
smallest game only has `Draw`. Call `Exit()` from anywhere to close the game.

## Why two kinds of update

Not every computer draws at the same speed such as a gaming PC may draw 144 frames per second or a browser tab on a laptop 30.
If your character moved a little every frame, it would run five times faster on the gaming PC. Khaos avoids that by
splitting the work in two:

- **`FixedUpdate`** is your gameplay. It always runs at the same rate, whatever the screen does: on a fast computer
  some frames skip it, on a slow one it runs twice in a frame to catch up. `time.Delta` is always the same value, so
  a jump is always the same height and a replay always ends the same way.
- **`Update`** runs once per frame, with the real time since the last frame. Use it for what does not change the
  game, only its look: a menu that slides in, a coin that spins.

When in doubt, put it in `FixedUpdate`.

So a faster screen never changes your game, only how smooth it looks: see [Frame rate](#frame-rate).

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
        _position += _velocity * (float)time.Delta;
    }

    protected override void Draw(Renderer renderer, in Frame frame)
    {
        // Where the ball is right now, between its last two positions.
        Vector2 drawn = Vector2.Lerp(_previous, _position, frame.Alpha);

        renderer.Clear(Color.Slate);
        // Draw the ball at `drawn`.
        renderer.Present();
    }
}
```

Without it, the game still works; things just move in small jumps when the rates differ.

## Frame rate

Khaos draws one frame each time the screen refreshes, which is called vertical sync (VSync): 60 frames per second
on a 60 Hz screen, 144 on a 144 Hz gaming monitor. More frames make movement smoother; gameplay keeps stepping at its
own steady rate in `FixedUpdate`, so the game itself does not change.

On the desktop, you can change that in your [settings](configuration.md), for example to save a laptop's battery:

```json
{
  "Graphics": {
    "VSync": true,
    "MaxFrameRate": 60
  }
}
```

| Setting        | What it does                                                                                                                                    | Default |
| :------------- | :---------------------------------------------------------------------------------------------------------------------------------------------- | :------ |
| `VSync`        | Waits for the screen before showing each frame. Turned off, the game draws as fast as it can, which can tear the picture and keep the fans busy | `true`  |
| `MaxFrameRate` | Never draws more frames per second than this. `0` means no limit                                                                                | `0`     |

In the browser, the page always draws at the screen's rate: browsers decide, and these settings do nothing there.

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

## Drawing happens in Draw

The `Renderer` you draw with is handed to `Draw`, and only there. You cannot draw from `Update` or from somewhere else
by mistake: keep your drawing code in `Draw`, and everything else outside it.

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

Load what takes time in `Load`, before the game starts, and split long work over several frames.
