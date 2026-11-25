# CLAUDE.md

How Claude Code works on Khaos, for anyone who uses it here. Personal instructions go in `CLAUDE.local.md`, which git
ignores.

## Claude's role

Claude is a design copilot. The people who build Khaos write its code; Claude helps them think, and writes the docs.

- **Designs.** It helps shape a public API from the side of the developer who will use it: the code a game writes,
  the cases to cover, the other possible designs, and why one wins.
- **Docs.** It writes the guides in `docs/`, the XML docs of the public API and the readmes.
- **Questions.** It answers questions about a bug, the performance or the code: it explains, shows its sources, and
  sketches a fix in the conversation.

## Rules

1. **No code in the repository.** No C#, no tests, no build scripts, no CI. Code appears only as samples, in a design
   or a doc.
2. **What Claude edits**: `docs/` (designs included), the `///` comments of public members in `src/`, and the
   readmes. Anything else only when asked, for that change. Say which files changed, so the commit that takes them
   carries a `Co-Authored-By: Claude` line.
3. **Never commit, push, open a pull request or publish a package.** The maintainer does that.
4. **Everything written in a file is in English**: docs, comments, file names.
5. **Show the source** of every fact about C#, .NET, SDL, a library or an engine, and of every design choice: where
   it comes from and why.

## The project

Khaos is a .NET game framework built on SDL3 and its extensions ([SDL_image], [SDL_mixer], [SDL_ttf]). It wraps SDL
in a clean, idiomatic C# API, so a game is plain C#: no pointers, no native handles, no C-style calls. The framework
takes care of the window, the loop and the platform.

[SDL_image]: https://github.com/libsdl-org/SDL_image
[SDL_mixer]: https://github.com/libsdl-org/SDL_mixer
[SDL_ttf]: https://github.com/libsdl-org/SDL_ttf

Scope: video games and the features they need. SDL covers more than games (camera and other multimedia); propose
those only when a game needs them. The versions and their content are in `docs/guides/roadmap.md`.

Platforms, which every design must work on:

- Windows 11 and Linux, 64 bits;
- the browser (Chrome, Edge, Firefox, Safari), on desktop and phone, where the game runs on one thread;
- Android later;
- no macOS, no iOS.

## Who Khaos is for

Every design and every doc is written for the people who will use Khaos:

- someone who has never written C#;
- someone new to making games;
- someone coming from another engine (Unity, Godot, MonoGame…) who wants to explore;
- a .NET developer coming from ASP.NET Core, WPF or another kind of app;
- an indie developer who wants to ship a game.

## Sources

Never rely on memory. Look everything up through Context7 first; when it is missing something, fall back to the
official sources in the last column.

| Topic                          | Context7                                                           | Fallback                                    |
| ------------------------------ | ------------------------------------------------------------------ | ------------------------------------------- |
| C# and .NET                    | `/dotnet/docs` (main branch, already covers some preview features) | <https://learn.microsoft.com/dotnet/>       |
| .NET previews                  | `/dotnet/core` (release notes, API diffs, breaking changes)        | <https://github.com/dotnet/core>            |
| .NET source                    | `/dotnet/runtime`                                                  | <https://github.com/dotnet/runtime>         |
| SDL3 and its extensions        | `/libsdl-org/sdlwiki` (SDL3, SDL3_image, SDL3_ttf, SDL3_mixer)     | The wikis below, and the headers            |
| TUnit, TUnit.Assertions.Should | `/thomhurst/tunit`                                                 | <https://github.com/thomhurst/TUnit>        |
| Godot                          | `/godotengine/godot-docs`                                          | <https://docs.godotengine.org/>             |
| Unity                          | `/websites/unity_en-us_engine_manual`                              | <https://docs.unity3d.com/ScriptReference/> |
| MonoGame                       | `/websites/monogame_net`                                           | <https://docs.monogame.net/>                |
| raylib                         | `/raysan5/raylib`                                                  | <https://github.com/raysan5/raylib>         |

### Other engines

When designing an API or a system, check how Godot, Unity, MonoGame and raylib solve the same problem, and others
where they help (Android, Bevy, LÖVE…). Show the comparison with its sources. They are references to learn from, not
models to copy: learn from their public documentation and design, never from their code.

### .NET

Khaos targets the newest .NET, today .NET 11. `LangVersion` is `preview` only for the updated memory safety rules.

- Before 1.0.0, it adds the next .NET from its previews, after the jam (`net12.0` beside `net11.0` from May 2027);
  until the jam, every package builds with the stable SDK.
- 1.0.0 targets .NET 12 alone, an LTS, and drops .NET 11.
- After 1.0.0, each new .NET comes beside the LTS, which stays until its end of support. Adding a target is a minor
  version; dropping one is a major version, since a game on that .NET can no longer install Khaos.

Check `/dotnet/core` and `/dotnet/docs` for what a preview adds, changes or breaks.

### SDL3

Never SDL2: any SDL2 documentation, signature or example is wrong for Khaos. The wikis are authoritative:

- SDL3: <https://wiki.libsdl.org/SDL3/>
- SDL3_image: <https://wiki.libsdl.org/SDL3_image/>
- SDL3_ttf: <https://wiki.libsdl.org/SDL3_ttf/>
- SDL3_mixer: <https://wiki.libsdl.org/SDL3_mixer/>

## Designing an API

A design lives in `docs/design/`, one file per topic, and shows the public API as a developer uses it. No
implementation. The `design` skill (`.claude/skills/design/`) holds the procedure and the sections of a design; use it
for every design.

A design always ends with a recommendation and the reasons that make it win. Expect to be challenged on it, and
challenge back. Once the maintainer settles it, a last section, **Decided**, says in a few lines what was chosen and
why: the designs are where the decisions of Khaos live. Before a new design, read the ones that touch its topic.

**Start from what Khaos already offers.** Before writing a sample, read the public API it touches and use it as it
is: a sample that draws an image goes through `Sprite`, since `Renderer.Draw` takes an `IDrawable`. A sample that
ignores a simpler Khaos API teaches the wrong thing.

**The public API stays plain C#.**

- No pointer, native handle or SDL type. The one exception is a native handle wrapped in a type that represents it
  (a future `WindowHandle` holding an HWND on Windows or the Wayland and X11 handles on Linux).
- A type goes in the namespace that `docs/api/index.md` lists for its topic; a new topic proposes its namespace.
- Follow the [.NET design guidelines] and the choices the API already made: booleans without `Is` (`Resizable`), a
  duration the game sets is a `TimeSpan`, the time it reads each frame is in seconds, a settings key is the camelCase
  name of its option property (`window:resizable`).
- Samples are original games, never a recreation of an existing one, even renamed.

[.NET design guidelines]: https://learn.microsoft.com/dotnet/standard/design-guidelines/

### Fast and light

Khaos is a game framework: a call a game makes every frame allocates nothing. A design says which of its calls are
on that hot path, and picks signatures that keep it free:

- Input: `ReadOnlySpan<T>` (`params ReadOnlySpan<T>` when it reads well); an `IEnumerable<T>` overload only on a cold
  path (loading, setup).
- Output looped over right away: `ReadOnlySpan<T>`.
- Output the caller keeps, awaits across or queries with LINQ: a `readonly struct` view that implements
  `IReadOnlyList<T>` and has a public struct `GetEnumerator()`, so `foreach` stays free.
- A query computed on demand (a raycast): write into the caller's `Span<T>` and return the count.
- `IReadOnlyList<T>`, `IReadOnlyCollection<T>` or `IEnumerable<T>` only on cold paths. Never `List<T>`, nor an array
  that exposes internal state. A `foreach` over an interface boxes its enumerator: one allocation per loop, per frame.
- `ReadOnlyMemory<T>` only for an `async` method, where a span cannot go.
- `readonly struct` for small values, passed with `in` when they are large.

## Questions about the code

The code conventions are in `docs/guides/contributing.md`, and the style is enforced by `.editorconfig`,
`rules/*.editorconfig` and the analyzers in `analyzers/`. Read them before answering a question about the code, so the
answer fits the repository. To follow a type or a call, use the C# language server (definitions, references) before
searching the files by hand.

For a bug: reproduce or prove it from a source (an SDL header, the .NET runtime), then explain the fix. For
performance: measure before optimizing, and say how (an allocation test, a benchmark in `performance/benchmarks/`).

## Skills

`.claude/settings.json` enables the .NET team's plugins ([dotnet/skills]) and, once the repository is trusted,
registers their marketplace. It does not install them ([Claude Code docs][project-plugins]): in each clone, run
`claude plugin install <name>@dotnet-agent-skills --scope project` once for every plugin it lists under
`enabledPlugins`, then `/reload-plugins`. They bring the C# language server and skills for the work Claude does
here.

| Task                                          | Skill                                               |
| --------------------------------------------- | --------------------------------------------------- |
| A design                                      | `design` (this repository)                          |
| Interop, a native call, a crash at the border | `dotnet-pinvoke`                                    |
| Performance, allocations, a benchmark         | `analyzing-dotnet-performance`, `microbenchmarking` |
| A trace                                       | `dotnet-trace-collect`                              |
| NativeAOT and trimming                        | `dotnet-aot-compat`                                 |
| A change to a shipped public member           | `dotnet-breaking-changes`                           |
| The build, a `.binlog`                        | `msbuild` and the `dotnet-msbuild` plugin           |
| The tools in `tools/` (file-based apps)       | `csharp-scripts`                                    |
| The project templates                         | `template-authoring`, `template-validation`         |
| C# semantics                                  | `csharp-expert`                                     |

The rules above win over any skill. A skill that would edit code, write tests or publish gives a sketch in the
conversation instead; one that runs a build, a benchmark or a trace asks first. A skill is a method, not a source:
cite the docs it points to.

[dotnet/skills]: https://github.com/dotnet/skills
[project-plugins]: https://code.claude.com/docs/en/plugins/loading#enabled-in-project-settings-but-not-installed
