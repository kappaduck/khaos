# CLAUDE.md

Guidance for Claude Code in the Khaos repository. Personal instructions go in `CLAUDE.local.md`, which git ignores.

## Rules

1. **The C# of the framework is written by hand.** Do not write or edit the repository's C#. Explain it, review it,
   or sketch an idea outside the repository instead (see the readme's AI disclosure).
2. **Edit the repository only when asked**, for that change: documentation, build scripts or CI. Say which files you
   changed, so the commit that takes them carries a `Co-Authored-By: Claude` line.
3. **Never commit, push, open a pull request or publish a package.** The maintainer does that.
4. **Everything written in a file is in English**: code, comments, docs, commit messages, file names.
5. **Cite your sources** for every piece of code and every fact about .NET, SDL or a library: the documentation,
   header or source that shows it is correct (signatures, behavior, limits).

## The project

Khaos is a .NET 11 game framework built on SDL3 and its extensions ([SDL_image], [SDL_mixer], [SDL_ttf]).

It wraps SDL in a clean, idiomatic C# API, so a game is plain C#: no pointers, no native handles, no C-style calls.
The framework takes care of the window, the loop and the platform.

[SDL_image]: https://github.com/libsdl-org/SDL_image
[SDL_mixer]: https://github.com/libsdl-org/SDL_mixer
[SDL_ttf]: https://github.com/libsdl-org/SDL_ttf

Goals, in order:

1. Make 2D games, ready for a game jam in April 2027.
2. Add 3D after the jam, and keep improving the framework.
3. Ship 1.0.0 by the end of 2027, or as close to it as possible.

Scope: video games and the features they need. SDL covers more than games (camera and other multimedia);
propose those only when a game needs them. No macOS, no iOS. The versions and their content are in
`docs/guides/roadmap.md`.

## Sources

Never rely on memory. Check every fact about C#, .NET, SDL or a library against a source, and show the
source in your answer.

Look everything up through Context7 first. When Context7 is missing something, fall back to the official
sources in the last column.

| Topic                          | Context7                                                            | Fallback                                       |
| ------------------------------ | ------------------------------------------------------------------- | ---------------------------------------------- |
| C# and .NET                    | `/dotnet/docs` (main branch, already covers some preview features)  | https://learn.microsoft.com/dotnet/            |
| .NET previews                  | `/dotnet/core` (preview release notes, API diffs, breaking changes) | https://github.com/dotnet/core                 |
| .NET source                    | `/dotnet/runtime`                                                   | https://github.com/dotnet/runtime              |
| SDL3 and its extensions        | `/libsdl-org/sdlwiki` (SDL3, SDL3_image, SDL3_ttf, SDL3_mixer)      | The wikis below, and the headers in the source |
| TUnit, TUnit.Assertions.Should | `/thomhurst/tunit`                                                  | https://github.com/thomhurst/TUnit             |
| Godot                          | `/godotengine/godot-docs`                                           | https://docs.godotengine.org/                  |
| Unity                          | `/websites/unity3d_manual`                                          | https://docs.unity3d.com/ScriptReference/      |
| MonoGame                       | `/websites/monogame_net`                                            | https://docs.monogame.net/                     |
| raylib                         | `/raysan5/raylib`                                                   | https://github.com/raysan5/raylib              |

### Other engines

When designing an API or a system, check how Godot, Unity, MonoGame and raylib solve the same problem, and show
the comparison with its sources. They are references to learn from, not models to copy.

### .NET

Khaos targets .NET 11 (release on November 10, 2026), then multi-targets .NET 12 preview from January or February
2027. Preview features are used often: check `/dotnet/core` and `/dotnet/docs` for what a preview adds,
changes or breaks.

### SDL3 and its extensions

Never SDL2. It is the old version; any SDL2 documentation, signature or example is wrong for Khaos.

The wikis are authoritative for native signatures:

- SDL3: https://wiki.libsdl.org/SDL3/
- SDL3_image: https://wiki.libsdl.org/SDL3_image/
- SDL3_ttf: https://wiki.libsdl.org/SDL3_ttf/
- SDL3_mixer: https://wiki.libsdl.org/SDL3_mixer/

Before proposing any binding, confirm from the wiki: the exact function name, the declaring header, the full
parameter list with types, the return type and its error convention, the ownership of every pointer, and the
SDL3 version that introduced it.

## Conventions

### Style

Read `.editorconfig`, `rules/*.editorconfig` and the analyzers in `analyzers/` before writing code, and follow
them at all times.

Every `.cs` file starts with this header:

```csharp
// Copyright (c) KappaDuck.
// Licensed under the MIT license.

```

### Folders and namespaces

Follow `docs/api/index.md` in the repository. It is a preview and may change: read it again before adding a type.

### Interop

SDL bindings stay in the `Interop` namespace and never leak into the public API: no pointers, no native
handles, no SDL types. The one exception is a native handle wrapped in a type that represents it. For example,
a `WindowHandle` that holds the handles SDL exposes for each platform (an HWND on Windows, the Wayland or X11
handles on Linux).

### Documentation

Everything public has XML documentation: classes, structs, enums and their values, properties, methods, events.
It is generated into the docfx site and the package. Markdown docs follow the style of `docs/` in the repository.
Both are written for the people who use Khaos:

- a C# developer who is new to games (from ASP.NET Core or elsewhere);
- a beginner who wants to make their first games;
- someone coming from Unity or another engine.

Be technical, but not too much. Do not mention SDL: it is an implementation detail. Naming it is acceptable only
in purely technical content, when the context calls for it.

Internal and private code has no comments, XML or inline. Let names carry the intent:
`if (HasTwoSwords())` rather than a comment above the `if`.

### Tests

Write tests with every sketch, test first (TDD). A test name starts with the member under test and reads as a
sentence: `FromDegreesShouldStoreTheAngleInRadians`.

A test method (marked `[Test]`) always has a block body, even for a single line, never an expression body (`=>`).
When the line gets long, put the inputs in local variables beside the call rather than switching to `=>`: a test
should read clearly.

### Performance

Hot paths should run with zero allocation. Prove it with a benchmark in `benchmarks/`, not by guessing (how
benchmarks are set up is still to be decided).

Khaos is a game framework: it must be fast and light. By default, write the code that allocates the least,
as long as it stays readable:

- take a sequence as `ReadOnlySpan<T>`, and `params ReadOnlySpan<T>` rather than `params T[]`: it promises the
  method only reads, and it accepts more (arrays, a `string`, another span, a collection expression). Take a
  `Span<T>` only for a destination the method writes into (`CopyTo(Span<T> destination)`, `TryWrite`);
- expose an internal array as `ReadOnlySpan<T>` rather than a copy;
- where a span cannot go (an `async` method, a buffer kept in a field of a class), use `ReadOnlyMemory<T>`
  (`Memory<T>` to write) and take `.Span` for the synchronous work. Keep it inside Khaos; in the public API only
  for an `async` method, where callers still pass an array. A method stops using a `Memory<T>` once it returns
  or its `Task` completes;
- slice with spans (`AsSpan`, ranges) rather than `Substring`, `Split` or a new array;
- put a small temporary buffer on the stack (`stackalloc` into a `Span<T>`, with a size limit and a heap
  fallback); rent a larger one from `ArrayPool<T>.Shared` and return it in a `finally`;
- no LINQ, closures or boxing in hot paths: plain `for` or `foreach` loops, `static` lambdas elsewhere;
- `readonly struct` for small values, passed with `in` when they are large;
- `ref struct` for a short-lived helper that holds a span or lives in one method (a builder, a parser, an
  enumerator): it never goes on the heap, and can hold `ref` fields;
- `SearchValues<T>` for repeated searches, `FrozenDictionary` and `FrozenSet` for lookups built once,
  `CollectionsMarshal.AsSpan` to read a `List<T>` without a copy.

Keep an array, an `IEnumerable<T>` or a plain struct when a span or a `ref struct` cannot work:

- a parameter of an `async` method (CS4012, use `ReadOnlyMemory<T>`), a value captured by a lambda or stored
  in a field of a class;
- data that goes through LINQ, or into an API that takes an array or an `IEnumerable<T>` (Roslyn,
  `ProcessStartInfo`);
- the analyzers, which target `netstandard2.0`.

Collections in the public API are chosen for a game loop. A `foreach` over an interface (`IEnumerable<T>`,
`IReadOnlyList<T>`) boxes the enumerator: one allocation per loop, every frame.

- Input: `ReadOnlySpan<T>` (`params` when it reads well). Add an `IEnumerable<T>` overload only on a cold path
  (loading, setup) where callers hold a lazy sequence.
- Output looped over right away (touches this frame, pressed keys): `ReadOnlySpan<T>`.
- Output the caller keeps, awaits across or queries with LINQ: a `readonly struct` view that implements
  `IReadOnlyList<T>` and has a public struct `GetEnumerator()`, so `foreach` stays free.
- Results computed on demand (queries such as a raycast): write into the caller's `Span<T>` and return the count.
- `IReadOnlyList<T>`, `IReadOnlyCollection<T>` or `IEnumerable<T>` (the narrowest that fits) only on cold paths.
- Never `List<T>`, nor an array that exposes internal state.

Measure before optimizing. No npm and no tricks:

- never `Volatile`;
- `goto` only when profiling proves it is worth it;
- plain tools such as `Interlocked.Increment` are fine.
