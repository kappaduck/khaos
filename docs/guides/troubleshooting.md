# Troubleshooting

Something does not work? Find what you see below. Each answer starts with the fix, then says why when it helps.

## When you build

### "The wasm-tools workload is not installed" (KHAOS103 or NETSDK1147)

Install the browser workload:

```bash
dotnet workload install wasm-tools
```

Updating the .NET SDK does not install or update workloads, so this can come back after an SDK update. See
[KHAOS103](../rules/khaos103.md).

### KHAOS100, KHAOS101 or KHAOS102

Errors from Khaos about the browser build. Each has a page with the fix:

| Error                            | In short                                                      | Fix                                       |
| :------------------------------- | :------------------------------------------------------------ | :---------------------------------------- |
| [KHAOS100](../rules/khaos100.md) | Khaos has no browser version for your game's .NET version     | Target `net11.0`, or update Khaos         |
| [KHAOS101](../rules/khaos101.md) | Your workload and Khaos were made for different .NET releases | `dotnet workload update`, or update Khaos |
| [KHAOS102](../rules/khaos102.md) | `KhaosWebCompression` has a typo                              | Use `Brotli`, `Gzip` or `None`            |

### The first browser build takes several minutes

That is normal: .NET builds a browser version of itself with Khaos inside. The next builds reuse it and take seconds.

## When you publish

### "Platform linker not found" (Windows)

Publishing a desktop game needs Visual Studio's C++ tools. Open the Visual Studio Installer and add the **Desktop
development with C++** workload, to Visual Studio or to the free Build Tools. See
[Installation](installation.md#when-publishing-a-desktop-game).

### clang or zlib is missing (Linux)

Install them, for example on Debian and Ubuntu:

```bash
sudo apt install clang zlib1g-dev
```

### "Cross-OS native compilation is not supported"

A desktop game is built for the system you publish on. Publish the Windows version on Windows and the Linux version
on Linux, or in a container: see [Publishing for Linux](linux.md).

## When the game runs on the desktop

### No window appears

If the environment variable `SDL_VIDEODRIVER` is set to `dummy`, the game draws in memory only, without a window: that
setting is for automated tests. Remove it. On Linux, make sure you run the game from a desktop session: through SSH
or in a container there is no screen to open a window on.

### "version 'GLIBC_2.41' not found" (Linux)

The system is older than what the game needs, or the game was published on a much newer system. See
[Publishing for Linux](linux.md).

## When the game runs in the browser

### The page stays blank, or shows an error

- Open the page through a web server: `dotnet run` for development, or a small server for a published game (see
  [Publishing](publishing.md#try-it-before-uploading)). Opening `index.html` from your disk does not work.
- Open the browser's developer tools (<kbd>F12</kbd>), **Console** tab: the error and where it happened are there,
  and on the page's error screen.

### The browser still shows the old version

Browsers keep the game's files in their cache. Reload without the cache with <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>R</kbd>,
or keep the developer tools open with **Disable cache** ticked in the **Network** tab.

### "Address already in use"

Another program uses the port of your game's local address. Change the port in `applicationUrl`, in
`MyGame.Web/Properties/launchSettings.json`.

### There is no sound

Browsers only allow a page to play sound after the player clicks or presses a key on it. That is why the page shows
a Play button: keep it, or make sure your own page asks for a click before the game starts.

### The tab freezes

A frame that waits or computes for a long time blocks the page. See
[Keep each frame short](game-loop.md#keep-each-frame-short).

## Still stuck?

Search the [issues on GitHub](https://github.com/kappaduck/khaos/issues), or open a new one with:

- where it happens: Windows, Linux, or the browser and which one;
- your Khaos version (`KhaosVersion` in `Directory.Packages.props`);
- what you ran, and the full error message.

A problem in the browser often does not happen on the desktop, and the other way around, so the platform matters.
