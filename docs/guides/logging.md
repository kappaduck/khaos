# Logs

Logs are short messages your game writes to tell you what it does: a level loaded, a save that failed, how long a
frame took. They show in the terminal on the desktop, and in the browser's developer console on the web.

Khaos uses the logging of .NET (`ILogger`), so if you have written an ASP.NET Core app, everything here works the same.

## Writing a log

Ask for an `ILogger<MyGame>` in the constructor of your game, then declare one method per message with
`[LoggerMessage]`:

```csharp
internal sealed partial class MyGame(ILogger<MyGame> logger) : Game2D("Ball", 1280, 720)
{
    private int _score;

    protected override void Initialize() => LevelLoaded("Forest");

    protected override void OnEvent(in Event e)
    {
        if (e is KeyPressed { Code: Scancode.Space })
        {
            _score += 10;
            Scored(_score);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Level {Level} loaded.")]
    private partial void LevelLoaded(string level);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Score is now {Score}.")]
    private partial void Scored(int score);
}
```

The class and the methods are `partial`, with no body: .NET writes the body while it compiles, and it uses the
`logger` of the constructor. Each `{Name}` in the message is replaced by the parameter with the same name.

This form allocates nothing, and costs almost nothing when its level is turned off, so you can call it from a method
that runs every frame. `logger.LogInformation("…")` works too, but it allocates on each call, and your editor suggests
the form above. See [compile-time logging](https://learn.microsoft.com/dotnet/core/extensions/logging/source-generation)
for everything `[LoggerMessage]` can do.

Your services log the same way: `HighScores(ILogger<HighScores> logger)`.

## Levels

Each message has a level, from the most detailed to the most serious:

| Level         | Shown as | Use it for                                                    |
| :------------ | :------- | :------------------------------------------------------------ |
| `Trace`       | `trace`  | Every small step, when you hunt a bug                         |
| `Debug`       | `debug`  | What helps while you develop: a score, a frame time           |
| `Information` | `info`   | What happens in a normal game: a level loaded, a game saved   |
| `Warning`     | `warn`   | Something odd the game recovers from: a missing sound file    |
| `Error`       | `fail`   | Something failed: a save could not be written                 |
| `Critical`    | `crit`   | The game cannot go on                                         |

## Where logs go

**On the desktop**, each log is one line in the terminal, with the time, the level in brackets and the message. The
messages all start in the same column. When a log carries an exception, the exception follows on the next lines.

```text
14:32:07.405 [info]:  Khaos 0.1.0 started on Windows with Direct3D 11 (native 3.4.20).
14:32:07.502 [info]:  Level Forest loaded.
14:32:09.118 [debug]: Score is now 10.
```

Each log also has a category, the full name of the class that asked for the logger, such as `MyGame.MyGame`. The line
does not show it, but it is the key you use to choose which logs show (below).

The level is in color in a terminal. `Warning` and above go to the error output, so you can keep problems apart from
the rest:

```bash
dotnet run --project MyGame.Desktop 2> problems.txt
```

Written to a file, logs have no color codes, unless you ask for them (below).

**In the browser**, open the developer tools (<kbd>F12</kbd>) and look at the **Console** tab. Each log is one
console message, with the level the console understands, so its level filter works:

| Level                 | In the console                                                                  |
| :-------------------- | :------------------------------------------------------------------------------ |
| `Trace`, `Debug`      | **Verbose** in Chrome and Edge, **Debug** in Firefox. Chrome and Edge hide it until you check it in the levels menu |
| `Information`         | **Info**                                                                        |
| `Warning`             | **Warnings**, in yellow                                                         |
| `Error`, `Critical`   | **Errors**, in red                                                              |

The message is the same line as on the desktop, without colors.

## How a line looks

Three settings change the line, in the `logging:console` section of your [settings](configuration.md):

| Setting           | Default        | What it does                                                                        |
| :---------------- | :------------- | :---------------------------------------------------------------------------------- |
| `timestampFormat` | `HH:mm:ss.fff` | The [format of the time](https://learn.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings) at the start of each line. Empty leaves the time out |
| `useUtcTimestamp` | `false`        | Writes the time in UTC rather than in the computer's time zone                      |
| `colors`          | `Auto`         | `Auto` colors the level in a terminal only, `Always` also in a file, `Never` never  |

```json
{
  "logging": {
    "console": {
      "timestampFormat": "HH:mm:ss",
      "colors": "Never"
    }
  }
}
```

`builder.Logging.ConfigureConsole` sets the same thing from `Program.cs`. The settings win over it, as for the rest of
the logging:

```csharp
KhaosApplicationBuilder builder = KhaosApplication.CreateBuilder(args);
builder.Logging.ConfigureConsole(console => console.UseUtcTimestamp = true);
```

A time format that .NET cannot read stops the game when it starts, with a message that names the setting.

### When the terminal is slow

On the desktop, a thread of its own writes the logs, so a slow terminal never slows the game down. The game only puts
each log in a queue, which takes a few dozen nanoseconds. In the browser, each log goes to the console right away.

| Setting          | Default | What it does                                                                                      |
| :--------------- | :------ | :------------------------------------------------------------------------------------------------ |
| `maxQueueLength` | `2500`  | How many logs can wait to be written. It must be 1 or more                                        |
| `queueFullMode`  | `Wait`  | What a log does when the queue is full. `Wait` makes the game wait for room, `Drop` drops the log and then writes how many were dropped |

The queue only fills when the game writes logs faster than the terminal shows them for a long time, such as a `Trace`
log in every frame. When the game exits, Khaos writes the logs still in the queue before it stops.

## Choosing which logs show

By default, Khaos shows `Debug` and above in the `Development` [environment](configuration.md#environments), and
`Information` and above in `Staging` and `Production`. Its own [native logs](#logs-from-khaos) start at `Warning`.

Change that in the `logging` section of your [settings](configuration.md). `default` applies to every category, and
a category or a namespace sets its own level:

```json
{
  "logging": {
    "logLevel": {
      "default": "Information",
      "MyGame.Physics": "Warning"
    }
  }
}
```

`MyGame.Physics` covers every class in that namespace, such as `MyGame.Physics.Collisions`. When several keys match a
category, the longest one wins. Set a level to `None` to hide a category entirely.

For one run, put the same key on the command line, or in the page address in the browser:

```bash
dotnet run --project MyGame.Desktop -- --logging:logLevel:default=Trace
```

```text
http://localhost:5123/?arg=--logging:logLevel:default=Trace
```

### From Program.cs

`builder.Logging` sets the same thing from code, handy for a condition:

```csharp
KhaosApplicationBuilder builder = KhaosApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
    builder.Logging.AddFilter("MyGame.Physics", LogLevel.Trace);

return builder.Build<MyGame>().Run();
```

`ConfigureLogging` does it in a chain:

```csharp
return KhaosApplication.CreateBuilder(args)
                       .ConfigureLogging(logging => logging.AddFilter("MyGame.Physics", LogLevel.Warning))
                       .Build<MyGame>()
                       .Run();
```

The `logging` section always wins over `Program.cs`, as for the window: a player can still turn logs up or down
without rebuilding.

`builder.Logging` also takes any other .NET logging provider, such as one that writes to a file. Khaos already writes
to the console, so do not add `AddConsole()`: every log would show twice, and it does not work in the browser. To set
a level for the console alone, use `logging:console:logLevel` in place of `logging:logLevel`.

## Logs from Khaos

Khaos logs under `KappaDuck.Khaos`. When the game starts, it writes one line under `KappaDuck.Khaos.Hosting` with its
version, the version of its native libraries, the video driver and the renderer. Copy that line into a bug report.

The native libraries under Khaos (SDL) have their own logs, under `KappaDuck.Khaos.Native` and one category per part:

| Category                          | What it covers                       |
| :-------------------------------- | :----------------------------------- |
| `KappaDuck.Khaos.Native.Video`    | The window and the screens           |
| `KappaDuck.Khaos.Native.Render`   | Drawing                              |
| `KappaDuck.Khaos.Native.Gpu`      | The graphics card                    |
| `KappaDuck.Khaos.Native.Audio`    | Sound                                |
| `KappaDuck.Khaos.Native.Input`    | Keyboard, mouse, gamepads and touch  |
| `KappaDuck.Khaos.Native.System`   | Startup, the platform and the rest   |
| `KappaDuck.Khaos.Native.Error`    | Errors                               |
| `KappaDuck.Khaos.Native.Assert`   | Failed checks inside the libraries   |

They start at `Warning`. When something goes wrong on one computer only, such as a black screen or a missing sound,
turn them down to see what the libraries chose:

```bash
dotnet run --project MyGame.Desktop -- --logging:logLevel:KappaDuck.Khaos.Native=Debug
```

```text
14:32:07.390 [debug]: SDL chose video backend 'windows'
14:32:07.391 [debug]: SDL chose render backend 'direct3d11'
14:32:07.402 [info]:  Created renderer: direct3d11
```

Native logs can come from another thread than the game's. Khaos writes each log whole, never mixed with another.
