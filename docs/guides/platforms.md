# Supported platforms

Your game runs on three platforms from the same code:

| Platform    | What players get                               | What they need                                    |
| :---------- | :--------------------------------------------- | :------------------------------------------------ |
| **Windows** | An executable to download and run              | Windows 10 or later, 64-bit                       |
| **Linux**   | An executable to download and run              | A recent 64-bit distribution: see [below](#linux) |
| **Browser** | A page to open, on a jam's page or any website | A recent browser: Chrome, Edge, Firefox or Safari |

Players never install .NET or anything else. The desktop executable carries everything, and the browser version
loads in the page.

Not supported: **macOS** (Mac players can play the browser version), 32-bit computers, and ARM desktops.
**Android** is planned after April 2027; iOS is not planned.

## What differs between them

Almost nothing, and that is the point. A few things are different in the browser, because a web page has rules a
desktop program does not:

|                                    | Desktop |                               Browser                               |
| :--------------------------------- | :-----: | :-----------------------------------------------------------------: |
| 2D drawing, input, sound, text     |   yes   |                                 yes                                 |
| Every image, sound and font format |   yes   |                                 yes                                 |
| Sound before the player clicks     |   yes   | no: browsers wait for a click, which is what the Play button is for |
| Running work in background threads |   yes   |           no: everything runs on the page's single thread           |
| 3D (after April 2027)              | planned |                                later                                |

## Linux

Linux distributions do not all ship the same version of their core system library, glibc, and a program only runs
on a version at least as new as the one it was built with. Khaos's libraries need **glibc 2.41**, which means:

- **Runs on**: Debian 13, Ubuntu 25.04 and later, Fedora 42 and later, Arch Linux and other rolling distributions.
- **Does not run on** (yet): Ubuntu 24.04 LTS, Debian 12, Fedora 41 and older.

Players on an older system can play the browser version. [Publishing for Linux](linux.md) explains how to make sure
your own build reaches the same systems.

## Code for one platform only

Sometimes you want something on one platform only, such as a menu bar on Windows or a link to the web page. Check
where the game runs with .NET's `OperatingSystem` class:

```csharp
if (OperatingSystem.IsBrowser())
{
    // Only in the browser.
}
else if (OperatingSystem.IsWindows())
{
    // Only on Windows.
}
```

Two good habits:

- **Put the check directly in the `if`**, as above, rather than saving it in a variable first. When you publish,
  .NET then leaves the other platforms' code out of the build entirely, so it costs nothing.
- **Listen to the warning.** When you call something that only exists on some platforms without such a check, the
  build warns you (warning `CA1416`) with the platforms it supports.

Platform-specific setup often fits best in the `Program.cs` of that platform's app, which only ever runs there.
