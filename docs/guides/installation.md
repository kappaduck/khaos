# Installation

You install three things, once per computer: the .NET SDK, the browser workload, and the Khaos template. Count about
ten minutes, most of it downloads. Everything else comes with Khaos.

## 1. The .NET 11 SDK

The SDK is what builds and runs C# programs. Download it from
[dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/11.0) for Windows x64 or Linux x64, install it,
then check it in a terminal:

```bash
dotnet --list-sdks
```

A line that starts with `11.0` means you are ready.

You also want an editor that knows .NET 11: **Visual Studio**, **JetBrains Rider**, or **Visual Studio Code** with
the C# Dev Kit extension. Any of them works with Khaos.

> [!NOTE]
> Khaos runs on Windows and Linux. macOS is not supported, neither to make games nor to play the desktop version;
> Mac players can still play the browser version.

## 2. The browser workload

To run your game in a browser, .NET needs an extra piece called the `wasm-tools` workload. It turns your game and
Khaos into WebAssembly, the format browsers run. Install it once:

```bash
dotnet workload install wasm-tools
```

On Linux, add `sudo` in front if .NET is installed in a system folder.

> [!TIP]
> Updating the .NET SDK does not update its workloads. After an SDK update, run `dotnet workload update` too, or the
> browser build may stop with an error that asks for it.

You can skip this step if you only make desktop games.

## 3. The Khaos template

```bash
dotnet new install KappaDuck.Khaos.Templates
```

This adds a **Khaos game** template to `dotnet new` and to the "new project" window of your editor.
[Your first game](getting-started.md) uses it right away.

The template has a few options:

| Option                  | What it does                                                                            |
| :---------------------- | :-------------------------------------------------------------------------------------- |
| `--platform both`       | The default: your game, plus an app for the desktop and an app for the browser          |
| `--platform desktop`    | One project, for Windows and Linux                                                      |
| `--platform browser`    | One project, for the browser: the quickest setup for a game jam                         |
| `--title "My Game"`     | The game's title, shown in the readme and the browser tab. Defaults to the project name |
| `--khaos-version 0.2.0` | Uses another Khaos version than the template's own                                      |

## When publishing a desktop game

When you publish for Windows or Linux, Khaos turns your game into a single native executable that players run
without installing .NET. That step, called NativeAOT, uses your system's C++ tools, which you install once:

- **Windows**: Visual Studio, or the free Build Tools for Visual Studio, with the **Desktop development with C++**
  workload. Tick it in the Visual Studio Installer.
- **Linux**: `clang` and the zlib headers, for example `sudo apt install clang zlib1g-dev` on Debian and Ubuntu.

You do not need them to make and test your game, nor to publish for the browser. [Publishing](publishing.md) has the
rest.

## Updating Khaos

A game made with the template keeps its Khaos version in one place: `KhaosVersion` in `Directory.Packages.props`.
Change the number there and every project of the game follows.

To get the newest template for your next game, run the install command again:

```bash
dotnet new install KappaDuck.Khaos.Templates
```

Preview versions (`0.x-beta`) are on NuGet next to the stable ones. To try one, put its number in `KhaosVersion`, or
install the template at that version with `dotnet new install KappaDuck.Khaos.Templates::<version>`.

## Without the template

The template is the easy way, but a Khaos game is an ordinary .NET project. Every project of a game references one
package, `KappaDuck.Khaos`, and Khaos sees what kind of project it is in:

- a project on `Microsoft.NET.Sdk` with `OutputType` `Exe` runs on Windows and Linux.
- a project on `Microsoft.NET.Sdk.WebAssembly` runs in the browser.

For the desktop:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <PublishAot>true</PublishAot>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="KappaDuck.Khaos" Version="0.1.0" />
  </ItemGroup>
</Project>
```

For the browser:

```xml
<Project Sdk="Microsoft.NET.Sdk.WebAssembly">
  <PropertyGroup>
    <TargetFramework>net11.0</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="KappaDuck.Khaos" Version="0.1.0" />
  </ItemGroup>
</Project>
```

To target both from the same code, put the game in a class library that references `KappaDuck.Khaos`, and give each
platform its own small project, as above, that references the library instead of the package. That is exactly what
the template does: see [Project structure](project-structure.md).
