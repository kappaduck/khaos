---
name: design
description: Design a public API or a system for Khaos, from the side of the developer who uses it. Use when asked to design, propose or compare an API for Khaos, or to write or update a file in docs/design/.
argument-hint: <topic>
---

# Designing an API for Khaos

The topic is `$ARGUMENTS`. A design lives in `docs/design/<topic>.md` and shows the public API as a developer uses it,
never its implementation. The rules it follows are in `CLAUDE.md` ("Designing an API", "Sources", "Other engines");
this skill is the procedure.

## 1. Read before writing

- The designs in `docs/design/` that touch the topic, their **Decided** section first. When the new design changes
  what one decided, say so and discuss it.
- `docs/design/asset-catalog.md` for the format, if no other design is open.
- `docs/api/index.md` for the namespace the topic belongs to; a new topic proposes its namespace.
- The public API the samples will touch, as it is today. Use the C# language server (definitions, references) to
  follow a type instead of guessing. A sample that ignores a simpler Khaos API teaches the wrong thing.

## 2. Research

Look everything up, never from memory, with the Context7 ids of the Sources table in `CLAUDE.md`.

- **Other engines**: how Godot, Unity, MonoGame and raylib solve the same problem, and others where they help
  (Android, Bevy, LÖVE…). Their public docs and design only, never their code. Note each link.
- **SDL3**: the functions that make it possible, and the SDL3 version that brings each ("This function is available
  since SDL 3.x.y" on the wiki). Never SDL2.
- **.NET**: the APIs that make it possible. Prefer .NET when it does as well as SDL: the fewer native calls, the
  better.

Skills that help along the way:

- `dotnet-aot-compat` when the design leans on reflection, generics over unknown types or anything trimming removes.
  Games publish with NativeAOT.
- `dotnet-breaking-changes` when the design changes a public member that has shipped.
- `dotnet-pinvoke` when a building block needs a new native call.

They are methods, not sources: cite the docs they point to.

## 3. Write

One file, these sections, in this order:

1. **The idea**: what the developer wants to do, and what goes wrong today.
2. **In use**: the code a game writes, from `Program.cs` to `Draw`.
3. **Cases to cover**: each situation the API must handle, with what the developer sees.
4. **Other designs**: two or three alternatives, each with its code and why it loses or wins, then **The
   recommendation** with the reasons that make it win.
5. **Other engines**: how they solve it, with links.
6. **Building blocks**: the SDL functions (with their SDL3 version) or the .NET APIs, each with its source.
7. **Open questions** for the maintainer.

While writing, check each call against "Fast and light" in `CLAUDE.md`: say which calls are on the per-frame hot path
and give them signatures that allocate nothing. Samples are original games, never a recreation of an existing one.

## 4. Settle

Expect the recommendation to be challenged, and challenge back with facts and sources. Once the maintainer settles
it, add **Decided**: a few lines on what was chosen and why.
