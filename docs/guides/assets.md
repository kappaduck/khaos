# Assets

Assets are the files your game uses that are not code like images, sounds, music, fonts. In Khaos they all go in one
folder, `assets/`, inside your game project, and Khaos takes care of bringing them along on every platform.

## Where to put them

```text
MyGame/
└─ assets/
   ├─ sprites/ball.png
   ├─ sounds/bounce.wav
   ├─ music/theme.ogg
   └─ fonts/pixel.ttf
```

Organize the folder however you like: subfolders are kept as they are. Your game loads `assets/sprites/ball.png` with
the same path on the desktop and in the browser.

You do not need to set anything up:

- **On the desktop**, the folder is copied next to the executable every time you build or publish.
- **In the browser**, it is packed inside the game and unpacked before the game starts.

Files whose name starts with a dot, such as `.gitkeep`, are left out.

## Load them by name, checked by the compiler

Khaos turns your `assets/` folder into C# properties. Declare a class for it once:

```csharp
[AssetCatalog]
internal static partial class Content;
```

Then use the properties instead of writing paths by hand:

```csharp
// Content.Sprites.Ball is assets/sprites/ball.png
Sprite ball = new(Assets.Load(Content.Sprites.Ball));
```

Your editor completes the names as you type. If you rename or delete a file, the build fails and points at every
line that used it, instead of the game crashing later when it reaches that line.

The catalog follows your assets folder wherever it is. To keep your files in another folder, set it in the game's
`.csproj`; your game still loads them as `assets/...`, and the catalog does not change:

```xml
<PropertyGroup>
  <KhaosAssetsDirectory>art/</KhaosAssetsDirectory>
</PropertyGroup>
```

To get a class for one subfolder only, name it: `[AssetCatalog("sprites")]` lists the files of `assets/sprites/`.

> [!NOTE]
> The asset catalog arrives with Khaos 0.3.0 (see the [roadmap](roadmap.md)).

## Which formats to use

Khaos reads the same formats on the desktop and in the browser, so an asset that loads on your computer loads for
every player:

| Kind   | Formats                                                                                                     |
| :----- | :---------------------------------------------------------------------------------------------------------- |
| Images | PNG and animated PNG, JPEG, WebP and animated WebP, GIF and animated GIF, QOI, BMP, TGA, SVG, ICO, CUR, ANI |
| Audio  | WAV, AIFF, Ogg Vorbis, Opus, FLAC, MP3, and tracker music (MOD, XM, S3M, IT)                                |
| Fonts  | TrueType and OpenType, including complex scripts (Arabic, Hindi…) and colour emoji                          |

When in doubt, these suit most games, and every tool can export them:

| For                | Use                                            |
| :----------------- | :--------------------------------------------- |
| Images and sprites | **PNG**                                        |
| Sound effects      | **WAV** or **Ogg Vorbis**                      |
| Music              | **Ogg Vorbis** or **MP3**                      |
| Fonts              | **TrueType** (`.ttf`) or **OpenType** (`.otf`) |

## Assets you did not make

Sprites, sounds and fonts from asset packs come with a license. Read it before you use them, especially in a jam:
some ask for credit in the game, some forbid commercial use. A credits screen or a line on your game's page is
usually enough.

Khaos itself is built on open-source libraries whose licenses travel with your game automatically (see
[Publishing](publishing.md#credits)).
