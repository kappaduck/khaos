---
paths:
  - "docs/**/*.md"
  - "**/readme.md"
---

# Writing the docs

## The guides in `docs/`

- Write for the people in "Who Khaos is for" (`CLAUDE.md`). Be technical, but not too much: say what a thing does for
  the developer before how it works inside.
- When a sample uses a C# feature a newcomer may not know (a record, a pattern, `in`, a primary constructor), link
  its page in the C# docs (https://learn.microsoft.com/dotnet/csharp/).
- Say only what exists. Only `docs/guides/roadmap.md` says what comes; no "arrives in 0.x" line anywhere else.
- Do not mention SDL: it is an implementation detail. Name it only in purely technical content, when the context
  calls for it.
- Build every sample from the public API as it is (see "Start from what Khaos already offers" in `CLAUDE.md`).
- Plain sentences without colons ("To move a maximized window, use `Restore` first"), unless the colon introduces a
  code block or a list.
- Follow the style of the guides already in `docs/guides/`.

## Readmes

A `readme.md` inside the repository (`performance/`, `native/`, `demo/`…) is for the people who work on Khaos. Keep
what helps them do the work; leave out what they already know or will see in the project. The main `readme.md` and
`docs/` are for the people who use Khaos.

## Designs

`docs/design/` follows the `design` skill (`.claude/skills/design/`). It is not part of the site: write freely about
what does not exist yet.
