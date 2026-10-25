# The browser

Your game runs in any recent browser, with nothing for players to install. This page explains how that works, how to
change the page around your game, and how to put it online.

## How it works

Browsers run a format called **WebAssembly**. When you build the browser app, .NET compiles your game, the .NET
runtime and Khaos together into WebAssembly, and puts them in a small website: an `index.html` page and the files it
loads. Opening the page downloads the game and starts it in a `<canvas>`, the HTML element games draw into.

That build needs the `wasm-tools` workload, installed once (see [Installation](installation.md#2-the-browser-workload)):

```bash
dotnet workload install wasm-tools
```

The first build takes a few minutes; the next ones reuse its work and are much faster.

## Running it

```bash
dotnet run --project MyGame.Web
```

The terminal prints a local address, such as `http://localhost:5123`: open it in your browser. To see logs and
errors, open the browser's developer tools (<kbd>F12</kbd>) and look at the **Console** tab.

## The page

The template's page, in `MyGame.Web/wwwroot/`, shows a few screens around your game:

1. **Loading**: the Khaos duck and a progress bar while the game downloads.
2. **Play**: a button the player clicks to start. Browsers only allow sound after a click, so this is where the
   game gets it.
3. **The game**, with a fullscreen button in the corner.
4. **An error screen**, if the game cannot start, with the details to copy into a bug report.

You can change all of it. The page is yours: it is plain HTML and CSS.

### Changing the colors

The screens use colors you can override in `wwwroot/css/app.css`, without touching anything else:

```css
:root {
  --khaos-background: #101820;   /* behind the game and the screens */
  --khaos-primary: #f2aa4c;      /* the Play button */
  --khaos-text: #ffffff;
}
```

The other variables (`--khaos-surface`, `--khaos-text-muted`, `--khaos-danger`, the radius values…) work the same way.
Any other CSS rule you write in `app.css` applies too.

### Changing the layout

Edit `wwwroot/index.html` freely. Khaos finds its pieces by their `data-khaos-*` attributes, and each piece is
optional:

| Attribute                                              | What it is                                      |
| :----------------------------------------------------- | :---------------------------------------------- |
| `data-khaos-host`                                      | The element that holds the game and its screens |
| `data-khaos-screen="loading"`, `"ready"`, `"failed"`   | The screen shown in each state                  |
| `data-khaos-play`                                      | The button that starts the game                 |
| `data-khaos-fullscreen`                                | The fullscreen button                           |
| `data-khaos-error-message`, `data-khaos-error-details` | Where an error is written                       |

Keep the `<script>` that loads `_khaos/khaos.js` and a `<canvas id="canvas">`. A page with only those two things
works: the game starts right away, without screens.

To go back to the default page, delete `wwwroot/index.html`: Khaos uses its own when there is none.

## Your assets

The `assets/` folder of your game is packed into the browser version, and your game loads files from it with the
same paths as on the desktop. Some formats are not available in the browser, to keep the download small: see
[Assets](assets.md#which-formats-to-use).

## Publishing

```bash
dotnet publish MyGame.Web -c Release
```

This makes `MyGame.zip`, in `MyGame.Web/bin/Release/net11.0/publish/`, with `index.html` at the root of the
zip: upload it as is to a game jam or any website. [Publishing](publishing.md#for-the-browser) walks through it.

To make the download faster, the zip also holds compressed copies of the files. The page uses them when the website
supports it and falls back to the normal files otherwise, so the same zip works everywhere. You can
change which copies are made in `MyGame.Web.csproj`:

```xml
<PropertyGroup>
  <KhaosWebCompression>Gzip</KhaosWebCompression>
</PropertyGroup>
```

| Value    | What the zip holds                                                          |
| :------- | :-------------------------------------------------------------------------- |
| `Brotli` | The files, and a Brotli copy of each: the smallest download. The default    |
| `Gzip`   | The files, and a Gzip copy of each, for a host that does not support Brotli |
| `None`   | Only the files: the smallest zip, the slowest download                      |

### Testing the published game

A published game must be opened through a web server: double-clicking `index.html` does not work, because browsers
block a page from loading files straight from your disk. Any small web server does the job, for example the .NET tool
`dotnet-serve`:

```bash
dotnet tool install --global dotnet-serve
dotnet serve -d MyGame.Web/bin/Release/net11.0/publish/wwwroot -o
```

## Your game in a bigger website

The browser app is a small website of its own, and it fits inside a bigger one: a page about your game, a portfolio,
a site with several games, or a Blazor app with its own pages. Publish the game, copy what is in `publish/wwwroot/`
into a folder of your site, for example `game/`, and show it with an `<iframe>` on any page:

```html
<iframe src="/game/" width="1280" height="720" allowfullscreen></iframe>
```

In a **Blazor** app, put that folder in the app's `wwwroot/` and use the `<iframe>` in any page or component: the rest
of the site stays Blazor, with its own pages, menus and services, and the game runs in its frame. The two can send
each other messages with the browser's `window.postMessage`, for example to save a score on your site.

> [!NOTE]
> Running a Khaos game right inside a Blazor component, without an `<iframe>`, is planned after April 2027.

## Starting the game yourself (advanced)

By default the page starts the game as soon as it loads. To start it at the moment you choose, for example from your
own button, add `data-autostart="false"` to the `<script>` that loads `_khaos/khaos.js`, and call `Khaos.start`:

```html
<canvas id="game"></canvas>
<button id="start">Start the game</button>

<script type="module" src="_khaos/khaos.js" data-autostart="false"></script>
<script type="module">
  document.querySelector('#start').addEventListener('click', async () => {
    await Khaos.start({ canvas: '#game', clickToPlay: false, args: ['--level', '2'] });
  });
</script>
```

| Option        | What it does                                                                   | Default                                 |
| :------------ | :----------------------------------------------------------------------------- | :-------------------------------------- |
| `canvas`      | The canvas the game draws on: the element, or a CSS selector                   | `#canvas`, created if the page has none |
| `clickToPlay` | Waits for the player to press Play (or Enter, or Space) before the game starts | `true`                                  |
| `args`        | Arguments for your game, as if typed on a command line                         | none                                    |

`clickToPlay: false` is right here: the player already clicked your button, so the browser allows sound. The first
call to `Khaos.start` starts the game; later calls do nothing more. If the game cannot start, the page shows its error
screen and the promise `Khaos.start` returns fails with the reason.

## Talking to the page (advanced)

A game can call JavaScript, and JavaScript can call the game, with .NET's own
[`[JSImport]` and `[JSExport]`](https://learn.microsoft.com/aspnet/core/client-side/dotnet-interop/): to save a score
in the page, share a link, or react to a button in your HTML. Put your JavaScript files in `wwwroot/` next to the
page.

If you prefer TypeScript, add the compiler to the browser app as a NuGet package. It needs neither Node.js nor npm:

```xml
<PackageReference Include="Microsoft.TypeScript.MSBuild" Version="7.0.1" PrivateAssets="all" />
```

## .NET versions and the browser build

Khaos ships its browser libraries for each .NET version it supports, built to match that version's `wasm-tools`
workload. When the two do not match, the build stops with a clear message instead of a confusing link error:

| Message                          | What to do                                                                                             |
| :------------------------------- | :----------------------------------------------------------------------------------------------------- |
| [KHAOS100](../rules/khaos100.md) | Your game targets a .NET version this Khaos does not support yet: target `net11.0`, or update Khaos    |
| [KHAOS101](../rules/khaos101.md) | The workload and Khaos were made for different versions: run `dotnet workload update`, or update Khaos |
| [KHAOS103](../rules/khaos103.md) | The workload is not installed: run `dotnet workload install wasm-tools`                                |
