# Tools

The scripts that prepare a release, update SDL, compare the benchmarks, publish the games of the site and pack Khaos
locally. Each one is a [file-based app](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps), run from the
repository root:

```bash
dotnet tools/release/prepare.cs
```

A script called with the wrong arguments prints its usage.

## The scripts

| Script                       | What it does                                                                                                                                            |
| :--------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `release/prepare.cs`         | Dates the `[Unreleased]` section of `changelog.md`, adds the version to the compatibility table of `readme.md`, and ships the unshipped analyzer rules. |
| `release/next.cs [x.y.z]`    | Raises `VersionPrefix` in `Directory.Build.props` (the next minor by default) and opens the next `[Unreleased]` section.                                |
| `release/check.cs tag`       | Checks that the release tag fits `VersionPrefix` and its channel (`v0.1.0`, or `v0.1.0-beta.N` for a pre-release).                                      |
| `release/check.cs branch`    | Checks that the release commit is on `main`, or on `release/x.y` for a patch.                                                                           |
| `release/check.cs latest`    | Says whether the tag is the highest stable version, and which one comes before it.                                                                      |
| `release/resolve-version.cs` | Gives the version to pack, with the next `beta.N` nuget.org does not have yet for a pre-release.                                                        |
| `native/update-sdl.cs`       | Looks for newer stable SDL releases, updates `native/sdl.env`, bumps the Native package and writes the body of the pull request.                        |
| `benchmarks/compare.cs`      | Compares two benchmark runs and writes what changed beyond the threshold. `--strict` fails when an allocation went up.                                  |
| `benchmarks/add-results.cs`  | Adds the comparison and the reports of `artifacts/benchmarks` to `docs/performance/index.md`.                                                           |
| `site/publish-games.cs`      | Publishes each demo and example for the browser into the built site, or the coming soon page when it has no code yet.                                   |
| `packages/pack-local.cs`     | Packs every package under a local version into `artifacts/local-feed` and installs the template from it.                                                |
| `packages/check-template.cs` | Creates and builds a game of each platform from the template, then the samples under "Usage" in the readmes.                                            |

`release/check.cs` reads `Tag`, `Prefix`, `Channel` and `GITHUB_SHA` from the environment.

## Testing

```bash
dotnet test --project tests/Khaos.Tools.Tests
```

No test calls GitHub or nuget.org.

## Conventions

- Exit codes are 0 when the work is done, 1 when it failed, and 2 for wrong arguments.
- An error goes to stderr as `error: …`, and becomes a `::error::` annotation in a workflow.
- A script that edits files checks everything before it writes one, so a failure leaves the repository as it was.
- A script starts with `#!/usr/bin/env dotnet`, since the pull request workflow builds every file that starts with
  that line, and has a name of its own across the folders, since it builds into `artifacts/` under its file name.
