# KHAOS_TITLE

A [Khaos](https://github.com/kappaduck/khaos) game, on the desktop and in the browser from the same code.

| Folder                     | What it is                                                                                             |
| :------------------------- | :----------------------------------------------------------------------------------------------------- |
| `MyGame/`                  | The game: its code, its settings (`appsettings.json`) and its `assets/`.                               |
| `MyGame.Desktop/`          | Runs the game on Windows and Linux: `Program.cs` sets the settings and starts it.                      |
| `MyGame.Web/`              | Runs the game in the browser, the same way. `wwwroot/` holds its page: `index.html` and `css/app.css`. |
| `Directory.Packages.props` | The Khaos version, in one place for every project.                                                     |
| `Directory.Build.props`    | What every project shares: net11.0, nullable reference types, implicit usings.                         |

The game references `KappaDuck.Khaos`, and each app references the game. Khaos brings the rest to every
project (its native libraries, the Khaos global usings, the assets folder).

## Desktop

```bash
dotnet run --project MyGame.Desktop
```

## Browser

Once per machine: `dotnet workload install wasm-tools`.

```bash
dotnet run --project MyGame.Web
```

Then open the address it prints.

## Publish for a game jam

```bash
dotnet publish MyGame.Web -c Release
```

`MyGame.Web-web.zip` appears in `MyGame.Web/bin/Release/net11.0/publish/`: upload it to itch.io as an
HTML game, with `index.html` at the root. It carries Brotli copies of the files, used when the host
serves them (itch.io does); set `KhaosWebCompression` to `Gzip` or `None` in `MyGame.Web.csproj` to change that.

Test the published folder before uploading: `npx serve` in `publish/wwwroot`.

## Environments

The game runs in `Development`, `Staging` or `Production`, read from `builder.Environment` like any .NET app.
A Debug build starts in Development, with the debug overlay on (F1 hides it); a Release build and
`dotnet publish` start in Production, where the in-game tools are not in the build at all. Staging is a
Release build that keeps them: F1 shows the frame times on the real host.

Settings that change without recompiling go in `MyGame/appsettings.json`, and those of one environment in
`appsettings.<environment>.json` next to it (`appsettings.Development.json` turns the overlay on and logs at
`Debug`). Both apps read them, the browser too. Environment variables prefixed `KHAOS_` and the command line
override them: `--Diagnostics:Overlay false`.

Each app has the three as launch profiles, in `Properties/launchSettings.json`, for your IDE and `dotnet run -lp`:

```bash
dotnet run --project MyGame.Desktop -lp Staging
dotnet run --project MyGame.Desktop -- --environment Production
```

In the browser, launch profiles cannot reach the game: add the environment to the address the browser app
prints, `http://localhost:KHAOS_HTTP_PORT/?arg=--environment&arg=Staging`, or build for it:

```bash
dotnet publish MyGame.Web -c Release -p:KhaosEnvironment=Staging   # a test build for an unlisted itch.io page
```

## Program.cs

Each app has its own `Program.cs`: it builds the application, sets the settings and runs the game. Anything
only one platform needs (a desktop menu bar, a page message bridge) goes there.
