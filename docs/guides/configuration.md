# Configuration and environments

This page covers two things that work together: **settings**, values you change without recompiling, and
**environments**, the way the same game behaves differently while you develop it and once players have it.

If you have written an ASP.NET Core or any modern .NET app, this works exactly the same way, on the desktop and in
the browser.

## Settings

Your game has an `appsettings.json` file next to its code, in `MyGame/`. Put there what you want to tune without
rebuilding: a volume, a difficulty, a log level.

```json
{
  "Game": {
    "StartingLives": 3,
    "MusicVolume": 0.8
  }
}
```

Read a value anywhere you have the configuration:

```csharp
int lives = builder.Configuration.GetValue<int>("Game:StartingLives");
```

For more than a couple of values, describe the section as a class and let .NET fill it in: that is the
[options pattern](https://learn.microsoft.com/dotnet/core/extensions/options).

The file goes with your game on every platform: next to the executable on the desktop, inside the game in the
browser. You read it the same way everywhere.

### Changing a setting for one run

A setting can be overridden without touching the file, which is handy for testing:

```bash
dotnet run --project MyGame.Desktop -- --Game:StartingLives 99
```

From strongest to weakest, Khaos reads:

1. the command line (`--Game:StartingLives 99`), or the page address in the browser (`?arg=--Game:StartingLives&arg=99`)
2. environment variables that start with `KHAOS_`, with `__` in place of `:` (`KHAOS_GAME__STARTINGLIVES=99`)
3. `appsettings.<environment>.json`, explained below
4. `appsettings.json`

## Environments

A game runs in one **environment**: `Development` while you work on it, `Production` once you publish it, and
`Staging` for a test build in between. You can check which one is active and act on it:

```csharp
if (builder.Environment.IsDevelopment())
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
```

Each environment can have its own settings file, `appsettings.Development.json` for example, read on top of
`appsettings.json`. The template already has one, which shows more logs and turns the overlay on while you develop.

|                             | Development                                            | Staging                                                                                     | Production                    |
| :-------------------------- | :----------------------------------------------------- | :------------------------------------------------------------------------------------------ | :---------------------------- |
| When                        | You run the game from your editor or with `dotnet run` | You ask for it                                                                              | You publish the game          |
| In-game tools (the overlay) | Included, shown from the start; <kbd>F1</kbd> hides it | Included, hidden; <kbd>F1</kbd> shows it                                                    | Not included at all           |
| Logs (template)             | Everything, down to `Debug`                            | `Information` and up                                                                        | `Information` and up          |
| Settings file               | `appsettings.Development.json`                         | `appsettings.Staging.json`                                                                  | `appsettings.Production.json` |
| Good for                    | Writing the game                                       | Testing a real build where you publish it, with <kbd>F1</kbd> to check the frame rate there | What players download         |

The in-game tools are left out of a Production build entirely, so they cost nothing to players and they cannot turn
them on.

> [!NOTE]
> The overlay comes with Khaos 0.2.0, and grows until 0.5.0 (frame graph, draw calls, logs). See the
> [roadmap](roadmap.md).

### Choosing the environment

**On the desktop**, pick a launch profile next to the Run button of your editor (Development, Staging or
Production), or from the terminal:

```bash
dotnet run --project MyGame.Desktop -lp Staging
dotnet run --project MyGame.Desktop -- --environment Production
```

**In the browser**, the environment goes in the page address. The template's launch profiles already do it when
you start from your editor; with `dotnet run`, add it yourself to the address it prints:

```text
http://localhost:5123/?arg=--environment&arg=Staging
```

**For a published build**, choose it when you publish. This is how you make a Staging build for a draft
page, with the overlay still inside:

```bash
dotnet publish MyGame.Web -c Release -p:KhaosEnvironment=Staging
```

> [!IMPORTANT]
> Changing the environment of a published game, from its address for example, does not bring the in-game tools back
> into a Production build: they are simply not in it.

## Services and logging

`Program.cs` is where you set up the rest of your app, like in any .NET program:

```csharp
KhaosApplicationBuilder builder = KhaosApplication.CreateBuilder(args);

builder.Services.AddSingleton<HighScores>();                  // a service your game can ask for
builder.Logging.AddFilter("MyGame.Physics", LogLevel.Warning); // quieter logs for one part of the game

return builder.Build<MyGame>().Run();
```

- **Services**: register a class once, and Khaos creates it and hands it to whoever needs it. Useful for things the
  whole game shares, such as high scores or save data.
- **Logs**: write messages with `ILogger` to see what your game does. They show in the terminal on the desktop, and
  in the browser's developer console (<kbd>F12</kbd>) on the web.

You do not have to use either: a small game can do without.
