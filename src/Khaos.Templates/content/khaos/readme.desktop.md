# KHAOS_TITLE

A [Khaos](https://github.com/kappaduck/khaos) game for Windows and Linux.

| Path | What it is |
| :--- | :--------- |
| `MyGame/` | The game: its code, its settings (`appsettings.json`) and its `assets/`. |
| `Directory.Packages.props` | The Khaos version. |
| `Directory.Build.props` | net11.0, nullable reference types, implicit usings. |

```bash
dotnet run --project MyGame
dotnet publish MyGame -c Release -r win-x64     # or linux-x64
```

## Environments

The game runs in `Development`, `Staging` or `Production`, read from `builder.Environment` like any .NET app.
A Debug build starts in Development, with the debug overlay on (F1 hides it); a Release build and
`dotnet publish` start in Production, where the in-game tools are not in the build at all. Staging is a
Release build that keeps them.

Settings that change without recompiling go in `MyGame/appsettings.json`, and those of one environment in
`appsettings.<environment>.json` next to it (`appsettings.Development.json` turns the overlay on and logs at
`Debug`). They are copied next to the executable. Environment variables prefixed `KHAOS_` and the command line
override them: `--Diagnostics:Overlay false`.

`MyGame/Properties/launchSettings.json` has the three as launch profiles, for your IDE and `dotnet run -lp`:

```bash
dotnet run --project MyGame -lp Staging
dotnet run --project MyGame -- --environment Production
dotnet publish MyGame -c Release -r win-x64 -p:KhaosEnvironment=Staging
```

## Adding the browser later

Move `Program.cs` to a new `MyGame.Desktop` project (`<OutputType>Exe</OutputType>`, `<PublishAot>true</PublishAot>`
and a reference to `MyGame`), remove `OutputType` and `PublishAot` from `MyGame.csproj` so it becomes a library, and
add a `MyGame.Web` project with `<Project Sdk="Microsoft.NET.Sdk.WebAssembly">`, `<OutputType>Exe</OutputType>`, its
own `Program.cs` and the same reference. Khaos comes to both through `MyGame`.
`dotnet new khaos --platform both` shows the result.
