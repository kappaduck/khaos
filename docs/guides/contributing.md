# Contributing

Khaos is built in the open at [kappaduck/khaos](https://github.com/kappaduck/khaos). This page sets up a clone of the
repository: building it, running its tests and its sandbox, previewing this documentation, and trying the packages
in a game of your own.

## What you need

The [installation](installation.md) of a game, plus Git. `global.json` pins the .NET 11 SDK the repository builds
with. SDL is never built on your machine: every build, the repository's included, takes it from the published
`KappaDuck.Khaos.Native` package.

## Build and test

```bash
git clone https://github.com/kappaduck/khaos.git
cd khaos

dotnet build khaos.slnx
dotnet test --solution khaos.slnx                                             # headless
dotnet test --solution khaos.slnx -- --test-parameter video-driver=default    # with real windows
```

Tests run headless by default, on SDL's `dummy` video driver. A change to windowing or input also needs a run with
real windows. An `SDL_VIDEODRIVER` variable that is already set wins over the test parameter.

## The sandbox

`sandbox/` is a scratch game with one file, `Program.cs`, compiled by a desktop app and a browser app. It is
git-ignored: the first build creates it from `Program.cs.template`, and what you write there stays on your machine.

```bash
dotnet run --project sandbox/Desktop
dotnet run --project sandbox/Web
```

## Examples and demos

The [examples](../examples/readme.md) and the [demos](../../demo/readme.md) build against `src/` directly, with the
package's own build files (`IsKhaosGame=true`, set by their `Directory.Build.props`), so a change to the framework
shows in them without packing anything:

```bash
dotnet run --project docs/examples/Desktop -p:Example=rendering/01-clear
dotnet run --project demo/Breakout/Breakout.Web
```

### Adding an example

```text
docs/examples/<category>/<NN>-<name>/
├─ <Name>.cs      the whole example, in one file: its game and its entry point
├─ readme.md      a title, one sentence, the iframe on /examples/<category>/<NN>-<name>/play/, what to look at
└─ assets/        only when the example needs some
```

Add it to `docs/examples/toc.yml`. The page includes the `.cs` file the CI compiles (`[!code-csharp[](...)]`), so the
code on the page cannot drift from the code that runs.

### Adding a demo

A demo is the shape `dotnet new khaos` creates, under `demo/<Name>/`: `<Name>/` (the game), `<Name>.Desktop/` and
`<Name>.Web/`, plus a `readme.md` with the iframe on `/demo/<Name>/play/`, listed in `demo/toc.yml` and `khaos.slnx`.

### How the pages play them

When the documentation is deployed, `docs.yml` publishes the browser app of each demo and example that has code, into
the `play/` folder next to its page. One without code gets `docs/play/coming-soon.html` instead. A pull request
builds the pages without the games, so the iframes are empty there and in `docfx --serve`.

## This documentation

The site is built with [DocFX](https://dotnet.github.io/docfx/), a local tool of the repository:

```bash
dotnet build src/Khaos/Khaos.csproj -c Release    # the API reference is generated from the assemblies
dotnet tool restore
dotnet docfx docs/docfx.json --serve
```

The site is then at `http://localhost:8080`. Guides are Markdown files in `docs/guides/`, listed in
`docs/guides/toc.yml`. The theme is in `docs/templates/kappaduck/`: `kappaduck.min.css` is the KappaDuck brand
stylesheet as it is, and `main.css` maps DocFX's template onto it.

## Try the packages locally

To use the code of your clone in a game of your own, as a game would from NuGet:

```bash
dotnet tools/pack-local.cs
```

It packs every package under a local version into `artifacts/local-feed`, installs the template from it, and prints
the commands that create a game using that feed.

## Diagnostics

Khaos has two analyzer assemblies, with their own prefixes and numbering:

| Prefix | Assembly | Runs for | Shipped |
| :----- | :------- | :------- | :------ |
| `KHI` | `KappaDuck.Khaos.Internal.Analyzers` | The source code of Khaos | Never |
| `KHAOS` | `KappaDuck.Khaos.Analyzers` | Games using Khaos | Inside the `KappaDuck.Khaos` package |

`KHAOS100` and up are build errors reported by the package's MSBuild targets, not by an analyzer. The `KHI` rules
(interop conventions: `bool` marshalling, no `nint`, explicit entry points…) have their pages in
[`docs/rules/`](https://github.com/kappaduck/khaos/tree/main/docs/rules): read them before touching
`src/Khaos/Interop/`. They are left out of the site's menu, since games never see them.

To add a rule:

1. Add the `DiagnosticDescriptor`, with a `helpLinkUri` pointing at `docs/rules/<id>.md`.
2. Add the row to `AnalyzerReleases.Unshipped.md`, or the build fails (RS2000).
3. Write the page: cause, why, reported, fix, not reported, suppressing. A `KHAOS` page is read by game authors:
   write it for them, and add it to `docs/rules/toc.yml` and `docs/rules/readme.md`.
4. Write the tests, including one with the shape the code has **after** the source generators have run: a test
   compilation runs no generator.

## Conventions

- **Code is written by hand.** AI helps with documentation and design exploration, not with the code of the
  repository (see the readme's AI disclosure).
- One branch, `main`. Changes arrive by pull request, with the template in `.github/`. Titles are short and in the
  past tense: `Added Mathematics module`.
- The style is enforced by the build (`.editorconfig`, `rules/`, `AnalysisLevel=preview-all`): explicit types, no
  `var`, file-scoped namespaces, the copyright header on every `.cs` file, XML documentation on every public member.
- Every change that players or game authors can see goes in `changelog.md`.
- The guides are written for people who use Khaos, from first-time game makers to jam veterans: say what a thing
  does for them before how it works inside.
