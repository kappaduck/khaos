# Your first game

In this page you create a game, run it on your computer and in your browser, change it, and make the file you would
upload to a game jam or a website. It takes about fifteen minutes, most of it the first build.

Before you start, do the [installation](installation.md): the .NET 11 SDK, the `wasm-tools` workload and the Khaos
template.

## 1. Create the game

Open a terminal in the folder where you keep your projects:

```bash
dotnet new khaos -o MyGame --title "My First Game"
cd MyGame
```

You get a folder with three projects:

```text
MyGame/
├─ MyGame/              your game: its code, its settings and its assets
├─ MyGame.Desktop/      runs the game on Windows and Linux
├─ MyGame.Web/          runs the game in the browser
└─ MyGame.slnx          open this in your editor
```

Your code goes in `MyGame/`. The two other projects are small launchers: they start the same game on their
platform, so you write it once. [Project structure](project-structure.md) explains every file when you are curious.

> [!TIP]
> Making a game for the browser only, for a jam? `dotnet new khaos -o MyGame --platform browser` creates a single
> project instead of three. See [Khaos for a game jam](game-jam.md).

## 2. Run it on your computer

```bash
dotnet run --project MyGame.Desktop
```

A window opens. Close it, or press <kbd>Esc</kbd> or <kbd>Alt</kbd>+<kbd>F4</kbd>, to stop the game.

In your editor, pick `MyGame.Desktop` as the startup project and press the Run button (<kbd>F5</kbd>): it does the
same, with the debugger attached.

## 3. Run it in your browser

```bash
dotnet run --project MyGame.Web
```

The first time, the build takes a few minutes: .NET prepares a version of itself that runs in the browser, with Khaos
inside. The next builds are much faster.

When it is ready, the terminal prints an address such as `http://localhost:5123`. Open it in your browser. You see a
loading screen, then a **Play** button: click it and the game starts. The button is there because browsers only let
a page play sound after the player clicks something.

## 4. Change something

Open `MyGame.slnx` in your editor and find your game class in `MyGame/`. It derives from `Game2D`, and its `Draw`
method fills the window with one color, every frame:

```csharp
protected override void Draw(Renderer renderer, in Frame frame)
{
    renderer.Clear(Color.Slate);
    renderer.Present();
}
```

Change `Color.Slate` to another color, run the game again, on the desktop or in the browser: it follows. Both apps
run the same game, so every change you make shows on both.

From here:

- [How a game runs](game-loop.md) explains the other methods your game can override: loading, input, updating,
  drawing.
- The [examples](../examples/readme.md) show one feature at a time, with code you can copy.

## 5. Publish it

"Publishing" means making the files you give to players.

For the browser:

```bash
dotnet publish MyGame.Web -c Release
```

You get `MyGame.zip` in `MyGame.Web/bin/Release/net11.0/publish/`. That zip is your game. Upload it to
a game jam or any website and anyone can play it in their browser.

For Windows or Linux, publish on that system:

```bash
dotnet publish MyGame.Desktop -c Release -r win-x64     # on Windows
dotnet publish MyGame.Desktop -c Release -r linux-x64   # on Linux
```

You get a folder with your game's executable and everything it needs; zip it and share it. Players do not need to
install .NET.

[Publishing](publishing.md) covers both in detail, with what to check before you upload.

> [!NOTE]
> Khaos is young: names in the code can still change from one version to the next until 1.0.

## Next steps

- [How a game runs](game-loop.md): the methods of your game, and when Khaos calls them.
- [Assets](assets.md): where to put images, sounds and fonts.
- [Configuration and environments](configuration.md): settings you change without recompiling.
