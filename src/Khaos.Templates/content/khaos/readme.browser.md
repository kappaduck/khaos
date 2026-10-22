# KHAOS_TITLE

A [Khaos](https://github.com/kappaduck/khaos) game for the browser.

| Path | What it is |
| :--- | :--------- |
| `MyGame/` | The game: its code, its settings (`appsettings.json`) and its `assets/`. |
| `MyGame/wwwroot/` | The page: `index.html` (the loading, Play and error screens) and `css/app.css`, the game's styles. Delete `index.html` for the default page. |
| `Directory.Packages.props` | The Khaos version. |
| `Directory.Build.props` | net11.0, nullable reference types, implicit usings. |

Once per machine: `dotnet workload install wasm-tools`.

```bash
dotnet run --project MyGame
dotnet publish MyGame -c Release
```

`MyGame-web.zip` appears in `MyGame/bin/Release/net11.0/publish/`: upload it to itch.io as an HTML game.
Test the published folder first: `npx serve` in `publish/wwwroot`.

## Environments

The game runs in `Development`, `Staging` or `Production`, read from `builder.Environment` like any .NET app.
A Debug build starts in Development, with the debug overlay on (F1 hides it); a Release build and
`dotnet publish` start in Production, where the in-game tools are not in the build at all. Staging is a
Release build that keeps them: F1 shows the frame times on the real host.

Settings that change without recompiling go in `MyGame/appsettings.json`, and those of one environment in
`appsettings.<environment>.json` next to it (`appsettings.Development.json` turns the overlay on and logs at
`Debug`). They are put in the page's file system, where the game reads them like on the desktop. Environment variables prefixed `KHAOS_` and the command line
override them: `--Diagnostics:Overlay false`.

`MyGame/Properties/launchSettings.json` has the three as launch profiles for your IDE, which opens the page
with the environment in its address. With `dotnet run`, add it yourself:
`http://localhost:KHAOS_HTTP_PORT/?arg=--environment&arg=Staging`. Or build for it:

```bash
dotnet publish MyGame -c Release -p:KhaosEnvironment=Staging   # a test build for an unlisted itch.io page
```
