---
paths:
  - "src/**/*.cs"
---

# XML docs of the public API

- Touch only the `///` comments of public types and members. Never a line of code, never an internal or private
  member (they have no comments at all).
- A quick reference: one or two sentences on what the member does, its unit and its default. A short value inline
  (`2560 × 1440`) is fine.
- Add a condition only when it changes what the developer does: two screens can share a name, so use `Key` to tell
  them apart; in the browser, the game always has a single screen.
- Leave out, even when true: code samples, performance and internals (allocations, caching), workflows that tie
  members together, edge cases such as `default` values. They go in a guide under `docs/`.
- Plain sentences without colons. Do not mention SDL.
- Write for the people in "Who Khaos is for" (`CLAUDE.md`), and write the shortest version first, then cut again.
