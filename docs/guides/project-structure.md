# Project structure

What `dotnet new khaos` creates, and what each file is for. You do not need to know all of it to make a game. The
first section is enough to find your way, the rest is there when you want to change something.

## The three projects

```text
MyGame/
├─ MyGame/                           your game
│  ├─ assets/                        images, sounds, fonts
│  ├─ appsettings.json               settings you can change without recompiling
│  ├─ appsettings.Development.json   settings used only while you develop
│  └─ MyGame.csproj
├─ MyGame.Desktop/                   runs the game on Windows and Linux
│  ├─ Program.cs
│  └─ Properties/launchSettings.json
├─ MyGame.Web/                       runs the game in the browser
│  ├─ Program.cs
│  ├─ wwwroot/                       the web page around the game
│  └─ Properties/launchSettings.json
├─ Directory.Build.props
├─ Directory.Packages.props
├─ MyGame.slnx
└─ readme.md
```

**`MyGame`** is your game, its classes, its [assets](assets.md) and its [settings](configuration.md#settings).
Almost all of your work happens here. It is a library, which means it does not run by itself. The two apps below
run it.

**`MyGame.Desktop`** and **`MyGame.Web`** are the apps that run your game, one per platform. Each has a short
`Program.cs` that starts the game. They are also the place for the rare thing only one platform has, such as a menu
bar on Windows or a link with the web page.

Why two apps instead of one? A desktop executable and a browser app are built in different ways, so each needs its
own project. Keeping your game in a library that both reference means you write it once and never copy code between
them.

With `--platform desktop` or `--platform browser`, you get a single `MyGame` project that is both the game and its
app. That is simpler when you only target one platform, for example in a game jam.

## The files at the root

| File                       | What it is for                                                                             |
| :------------------------- | :----------------------------------------------------------------------------------------- |
| `MyGame.slnx`              | The solution: open it in Visual Studio, Rider or VS Code                                   |
| `Directory.Packages.props` | The Khaos version, in one place: change `KhaosVersion` to update every project at once     |
| `Directory.Build.props`    | Settings every project shares: the .NET version, nullable reference types, implicit usings |
| `readme.md`                | A short guide to your game's folders and commands, to keep or replace                      |

## The web page

`MyGame.Web/wwwroot/` holds the page your game runs in `index.html` and `css/app.css` for your own styles. Out of
the box it shows a loading screen, then a Play button, the game, and a fullscreen button. You can change anything in
it: [The browser](browser.md#the-page) explains how.

## Launch profiles

`Properties/launchSettings.json` in each app gives your editor three ways to start the game with **Development**,
**Staging** and **Production**. Pick one next to the Run button. What changes between them is explained in
[Configuration and environments](configuration.md).

## What Khaos gives every project

When a project references `KappaDuck.Khaos`, directly or through your game, Khaos sets it up for you. That is why the
`.csproj` files are so short.

- **No `using` lines for Khaos.** The Khaos namespaces (`KappaDuck.Khaos`, `.Graphics`, `.Input`, `.Audio` and the
  others), plus the usual .NET ones for services and logging, are available in every file.
- **Your assets travel with the game.** The `assets/` folder of the game is copied next to the desktop executable,
  and put inside the browser app, without any setting.
- **Your settings too.** `appsettings*.json` go with the game on both platforms.
- **Warnings that matter for games.** Khaos turns on the warnings that point to code that would break once the game
  is published (see [Publishing](publishing.md#before-you-upload)), and warns when a texture, a sound or a font is
  created and never disposed, since that memory is not freed by the garbage collector.
- **An icon.** The desktop executable gets the Khaos icon until you give it your own (see
  [Publishing](publishing.md#for-windows-and-linux)).

## Reference: settings you can change

These go in a `<PropertyGroup>` of a `.csproj`, or of `Directory.Build.props` for every project. You rarely need
them.

| Setting                | Default                                               | What it does                                                                               |
| :--------------------- | :---------------------------------------------------- | :----------------------------------------------------------------------------------------- |
| `KhaosAssetsDirectory` | `assets/`                                             | The folder your assets are in, if you prefer another name                                  |
| `KhaosEnvironment`     | `Development` in Debug, `Production` when you publish | The environment the game starts in. See [Configuration and environments](configuration.md) |
| `KhaosDiagnostics`     | on, except in `Production`                            | Keeps the in-game tools (the overlay) in the build                                         |
| `KhaosImplicitUsings`  | `true`                                                | `false` removes the Khaos namespaces from the automatic `using` lines                      |
| `KhaosWebCompression`  | `Brotli`                                              | Browser app: how the published zip is compressed. See [The browser](browser.md#publishing) |
