# Khaos for a game jam

A game jam gives you a weekend, sometimes less, to make a whole game. Khaos is made with that in mind where a new game runs
in one command and another makes the zip you upload. This page is the short path, from the week before the jam to
the submission.

Most jam players play in their browser, straight from the jam's page, so this page focuses on a browser game. You can
still offer a desktop download next to it.

## Before the jam

Do this a few days before, not on Friday night. Each step takes a few minutes now and can cost an hour during the jam.

1. **Install everything**: the .NET 11 SDK, the `wasm-tools` workload and the Khaos template. See
   [Installation](installation.md).
2. **Make a test game and publish it all the way:**

   ```bash
   dotnet new khaos -o JamTest --platform browser
   cd JamTest
   dotnet run --project JamTest           # opens in your browser: check it runs
   dotnet publish JamTest -c Release      # makes JamTest.zip
   ```

   Upload the zip where your jam takes games (see [Where your game goes](#where-your-game-goes)), to a draft or
   unlisted page, and play it there. If that works, nothing will surprise you on Sunday.
3. **Read your jam's rules.** Most jams allow any engine, but some have rules about code or assets made before the
   jam, and about what must be made during it.
4. **Get your tools ready**: an image editor for sprites (Aseprite, LibreSprite, Krita…), a sound effect generator
   (such as jsfxr), a font you are allowed to use, and a level editor if your game needs maps (Tiled, LDtk).
5. **Look at the [examples](../examples/readme.md)** for the features you plan to use: you will copy from them a lot.

## Starting the game

When the theme is announced:

```bash
dotnet new khaos -o MyJamGame --platform browser --title "My Jam Game"
```

`--platform browser` makes a single project: the simplest setup when the browser is your only target. If you also
want a Windows or Linux download, leave it out to get the game plus a desktop app and a browser app.

Then put your game under version control (`git init`, and commit often). Being able to go back to the version from
an hour ago saves jams.

## While you make it

- **Test in the browser regularly**, not only at the end: every hour or two, run the browser version and play it for
  a minute. Some things behave differently there, such as which [file formats](assets.md#which-formats-to-use) load.
- **Use PNG, WAV, Ogg Vorbis and TrueType fonts.** They work everywhere.
- **Put gameplay in `FixedUpdate`.** Your game then plays the same on your PC and on a player's slow laptop. See
  [How a game runs](game-loop.md).
- **Watch the overlay.** While you develop, the overlay shows your frame rate (<kbd>F1</kbd> toggles it). If it
  drops on your computer, it will drop more in a player's browser.
- **Keep the Play button.** Browsers block sound until the player clicks; the template's page already handles it.
- **Pick a size and stick to it.** A 16:9 size such as 1280 × 720 or 960 × 540 fits jam pages well (Ludum Dare
  shows games in a 16:9 frame); decide early, since your art depends on it.

## Submitting

Leave yourself at least an hour before the deadline. Uploading, filling in the page and fixing the one thing that
broke always takes longer than planned.

1. Publish:

   ```bash
   dotnet publish MyJamGame -c Release
   ```

2. [Try the published game](publishing.md#try-it-before-uploading) once in your browser.
3. Upload `MyJamGame.zip` where your jam takes games: see below.
4. Add a cover image, a short description, the controls, and your credits (see [Publishing](publishing.md#credits)).
5. Make the page public and submit it to the jam.

## Where your game goes

The zip is the same everywhere: a small website with `index.html` at its root. Each jam says where it wants it.

| Where          | How                                                                                                                                                                                                                                       |
| :------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Ludum Dare** | On your entry's page on ldjam.com, upload the zip in the **Embedding** section: players then play it on the page, in a 948 × 533 frame. That frame cannot save anything in the browser or reach other websites.                           |
| **Other jams** | Most jams that take browser games take the same zip: upload it as a browser (HTML) game, set the frame to your game's size if the jam asks, and play it once on the page before you submit. See [Publishing](publishing.md#uploading-it). |

Check your jam's page for the size limit and the deadline in your time zone.

## A desktop download too

If you created the game without `--platform browser`, you also have a desktop app. Publish it on Windows for a
Windows download (see [Publishing](publishing.md#for-windows-and-linux)), zip the folder, and upload it next to the
browser version on the same page. Players who prefer a download, or whose browser struggles, will thank you.

## After the jam

Most jams keep a version of the game frozen for judging, and allow a separate post-jam version. Keep the jam zip,
and publish fixes as a new upload marked as post-jam.
