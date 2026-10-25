# Publishing

Publishing makes the files you give to players such as a zip to upload for the browser, a folder with an executable for
Windows and Linux. A published game is built for speed, without the debug tools you use while developing.

## For the browser

```bash
dotnet publish MyGame.Web -c Release
```

In `MyGame.Web/bin/Release/net11.0/publish/` you find:

|              | What it is                                                             |
| :----------- | :--------------------------------------------------------------------- |
| `MyGame.zip` | **Your game, ready to upload.** `index.html` is at the root of the zip |
| `wwwroot/`   | The same files, unzipped, to test before uploading                     |

### Try it before uploading

Open the published game in your browser once, to catch a missing asset or a format the browser does not read. It
needs a small web server: double-clicking `index.html` does not work. For example, with the .NET tool `dotnet-serve`:

```bash
dotnet tool install --global dotnet-serve
dotnet serve -d MyGame.Web/bin/Release/net11.0/publish/wwwroot -o
```

### Uploading it

The zip is a small website with `index.html` at its root, so a jam or a website that takes a browser game as a zip
takes it as is. Wherever it goes:

1. Upload `MyGame.zip` as a browser (HTML) game.
2. If the site asks for the size of the game's frame, give your game's size (for example 1280 × 720).
3. Keep the page private or as a draft first, open it, play it once, then make it public.

**On Ludum Dare**, the zip goes in the **Embedding** section of your entry's page on ldjam.com. Players then play it
right on the page, in a 948 × 533 frame, and that frame cannot save anything in the browser or reach other websites.
See [Khaos for a game jam](game-jam.md#where-your-game-goes).

> [!TIP]
> Want the overlay on the real page, to check the frame rate where players will play it? Publish a Staging build
> to a draft page: `dotnet publish MyGame.Web -c Release -p:KhaosEnvironment=Staging`, then <kbd>F1</kbd> in the
> game. See [Configuration and environments](configuration.md#choosing-the-environment).

## For Windows and Linux

A desktop game is published as a native executable where players double-click it, without installing .NET. That step
needs the C++ tools of your system, installed once (see
[Installation](installation.md#when-publishing-a-desktop-game)), and it builds for the system you run it on:

```bash
dotnet publish MyGame.Desktop -c Release -r win-x64     # on Windows
dotnet publish MyGame.Desktop -c Release -r linux-x64   # on Linux
```

To build the Windows version you need a Windows computer, and a Linux computer (or a container) for the Linux
version. [Publishing for Linux](linux.md) shows how to build the Linux version so it runs on as many systems as
possible.

The result is in `MyGame.Desktop/bin/Release/net11.0/win-x64/publish/` (or `linux-x64`):

|                                                                  | What it is                                 |
| :--------------------------------------------------------------- | :----------------------------------------- |
| `MyGame.exe` on Windows, `MyGame` on Linux                       | The game                                   |
| `SDL3`, `SDL3_image`, `SDL3_ttf`, `SDL3_mixer` (`.dll` or `.so`) | The libraries Khaos runs on                |
| `assets/`                                                        | Your [assets](assets.md)                   |
| `appsettings*.json`                                              | Your [settings](configuration.md#settings) |
| `licenses/`                                                      | The licenses of the libraries inside Khaos |

**Zip the whole folder** and share that zip. The `.pdb` or `.dbg` files are only for debugging and can stay out.

### Your game's name and icon

What players get is named after your game, without the `.Desktop` or `.Web` of the apps like `MyGame.exe` (or `MyGame`
on Linux), and `MyGame.zip` for the browser. To use another name, set `KhaosGameName` in the app's `.csproj`:

```xml
<PropertyGroup>
  <KhaosGameName>SuperPong</KhaosGameName>
</PropertyGroup>
```

The executable has the Khaos icon until you give it yours. Put an `.ico` file next to `MyGame.Desktop.csproj` and
add:

```xml
<PropertyGroup>
  <ApplicationIcon>icon.ico</ApplicationIcon>
</PropertyGroup>
```

On Windows, that icon shows on the file in the Explorer, on the game's window and in the taskbar. An `.ico` file
holds the same picture at several sizes (16, 32, 48 and 256 pixels); most image editors can export one.

On Linux, an executable has no icon of its own: Khaos sets the window's icon from a PNG instead, the Khaos duck until
you give it yours. Put a square PNG (256 × 256 works well) next to the `.ico` and add:

```xml
<PropertyGroup>
  <KhaosWindowIcon>icon.png</KhaosWindowIcon>
</PropertyGroup>
```

## Before you upload

- **Play the published version**, not only the one from your editor. A published game is optimized differently, and
  a few problems only show there.
- **Read the build's warnings.** Warnings whose code starts with `IL2` or `IL3` point to code that may work in your
  editor but break in the published game, usually code that relies on reflection. Fix them before you ship.
- **Credit what you used**, see below.

## Credits

Khaos is built on open-source libraries, such as SDL for the window and the libraries it uses for images, sound
and fonts. Their licenses
are copied into your published game automatically, in the `licenses/` folder: keep it in the zip.

If your game shows text, the FreeType library asks for a line in your game's credits or on its page:

> Portions of this software are copyright © The FreeType Project (www.freetype.org).

Khaos opens your game with a short "Made with Khaos" splash screen. Keeping it is a nice way to credit Khaos, as is a
mention on your game's page, but neither is required. To turn the splash screen off, add this to your
`appsettings.json`:

```json
{
  "Splash": {
    "Enabled": false
  }
}
```
