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

Pull requests also measure line coverage on Linux and on Windows, then merge both runs into one report, so a line
counts as covered when either platform runs it. The check fails below 85%. A comment on the pull request sums up
the coverage of each assembly, and the full report is in the run's `coverage-report` artifact. To measure coverage
on your machine, add `--coverage`.

```bash
dotnet test --solution khaos.slnx -- --coverage --coverage-output-format cobertura
```

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
dotnet docs/examples/rendering/clear/clear.cs
dotnet run --project docs/examples/Web -p:Example=rendering/clear
dotnet run --project demo/Breakout/Breakout.Web
```

### Adding an example

```text
docs/examples/<category>/<name>/
├─ <name>.cs      the whole example, in one file: its game and its entry point
├─ readme.md      a title, one sentence, the iframe on /examples/<category>/<name>/play/, what to look at
└─ assets/        only when the example needs some
```

An example is a [file-based app](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps):
`dotnet docs/examples/<category>/<name>/<name>.cs` runs it on the desktop, and `docs/examples/Web` compiles it for the
browser with `-p:Example=<category>/<name>`.

The example starts as soon as its page loads. One with sound or input points its iframe at `play/?click` instead,
so it waits for Play: the browser needs a click before it lets a page play sound or read the keyboard.

Add it to `docs/examples/toc.yml`, in the order to read them. The page includes the `.cs` file the CI compiles
(`[!code-csharp[](...)]`), so the code on the page cannot drift from the code that runs.

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

| Prefix  | Assembly                             | Runs for                 | Shipped                              |
| :------ | :----------------------------------- | :----------------------- | :----------------------------------- |
| `KHI`   | `KappaDuck.Khaos.Internal.Analyzers` | The source code of Khaos | Never                                |
| `KHAOS` | `KappaDuck.Khaos.Analyzers`          | Games using Khaos        | Inside the `KappaDuck.Khaos` package |

`KHAOS100` and up are build errors reported by the package's MSBuild targets, not by an analyzer. The `KHI` rules
(interop conventions: `bool` marshalling, no `nint`, explicit entry points…) have their pages in
[`docs/rules/`](https://github.com/kappaduck/khaos/tree/main/docs/rules): read them before touching
`src/Khaos/Interop/`. They are left out of the site's menu, since games never see them.

To add a rule:

1. Add the `DiagnosticDescriptor`, with a `helpLinkUri` pointing at `docs/rules/<id>.md`.
2. Add the row to `AnalyzerReleases.Unshipped.md`, or the build fails (RS2000). The row stays there through the
   betas. When a stable version is prepared, `dotnet tools/prepare-release.cs` moves it to
   `AnalyzerReleases.Shipped.md`, under `## Release x.y.z`.
3. Write the page: cause, why, reported, fix, not reported, suppressing. A `KHAOS` page is read by game authors:
   write it for them, and add it to `docs/rules/toc.yml` and `docs/rules/readme.md`.
4. Write the tests, including one with the shape the code has **after** the source generators have run: a test
   compilation runs no generator.

## Patch releases

A fix for a version that is already out ships as a patch (`0.1.1`) from a `release/x.y` branch, while `main` moves
on to the next minor. The branch starts from the release tag the first time a patch is needed, and serves every
patch of that minor. Only the latest minor gets patches: `release/0.1` stops when `0.2.0` ships.

A fix lands on `main` first, then is copied to the release branch with `git cherry-pick -x`, which records the
original commit in the message. Nothing is merged back from a release branch: the code is already on `main`, and
only the changelog follows.

1. Fix the bug on `main` with a pull request, as usual, and note the commit it was squashed into.
2. The first time, create the branch from the tag and push it.

   ```bash
   git switch -c release/0.1 v0.1.0
   git push -u origin release/0.1
   ```

3. Copy the fix on a branch of its own. The first fix of a patch also opens its `[Unreleased]` section.

   ```bash
   git switch -c backport/42-window-title origin/release/0.1
   dotnet tools/prepare-release.cs --next 0.1.1    # first fix of the patch only
   git commit -am "Opened 0.1.1"
   git cherry-pick -x 7f3c9e1
   ```

   `changelog.md` usually conflicts, since the entry was written under the next minor on `main`. Move it to the
   `[Unreleased]` section of the patch, then run `git add changelog.md` and `git cherry-pick --continue`. To start
   over, run `git cherry-pick --abort`.

4. Open the pull request against `release/0.1`, with the line in its title (`[0.1] Fixed Window.Title with accents`)
   and a link to the original pull request in its description.
5. When the patch is ready, run `dotnet tools/prepare-release.cs` on a branch of `release/0.1` and merge its pull
   request.
6. Draft a GitHub release with the tag `v0.1.1`, targeting `release/0.1`. If a later minor is already out, clear
   **Set as the latest release**. The workflow publishes the packages, and leaves the documentation site on the
   latest version.
7. On `main`, move the entry from the `[Unreleased]` section of `changelog.md` to a `0.1.1` section, and add the
   `0.1.1` row of the compatibility table in `readme.md`.

## Conventions

- **The C# of the framework is written by hand.** AI may help with design exploration, documentation, build scripts
  and CI, and reviews; a commit with AI-written changes says so with a `Co-Authored-By` line (see the readme's AI
  disclosure).
- Work happens on `main`, and the patches of a released version on `release/x.y` (see
  [Patch releases](#patch-releases)). Changes arrive by pull request, with the template in `.github/`. Titles are short and in the
  past tense: `Added Mathematics module`.
- The style is enforced by the build (`.editorconfig`, `rules/`, `AnalysisLevel=preview-all`): explicit types, no
  `var`, file-scoped namespaces, the copyright header on every `.cs` file, XML documentation on every public member.
- Every change that players or game authors can see goes in `changelog.md`.
- The guides are written for people who use Khaos, from first-time game makers to jam veterans: say what a thing
  does for them before how it works inside.
