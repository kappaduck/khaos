# Diagnostics

Khaos ships two analyzer assemblies, and they do not share a prefix.

| Prefix  | Assembly                             | Runs for          | Shipped                              |
| :------ | :----------------------------------- | :---------------- | :----------------------------------- |
| `KHI`   | `KappaDuck.Khaos.Internal.Analyzers` | Khaos itself      | Never                                |
| `KHAOS` | `KappaDuck.Khaos.Analyzers`          | Games using Khaos | Inside the `KappaDuck.Khaos` package |

A diagnostic in a bug report, an `.editorconfig` or a suppression therefore says
which side reported it without anyone having to look it up. The two series are
numbered independently: `KHI001` and `KHAOS001` are unrelated.

## Internal rules `KHIXXX`

| ID                    | Rule                                                                              | State       |
| :-------------------- | :-------------------------------------------------------------------------------- | :---------- |
| [`KHI001`](khi001.md) | bool return value or parameter does not follow the convention of its interop area | Implemented |
| [`KHI002`](khi002.md) | `nint` or `IntPtr` in an interop declaration                                      | Implemented |
| [`KHI003`](khi003.md) | `[LibraryImport]` without an explicit `EntryPoint`                                | Implemented |
| [`KHI004`](khi004.md) | `void*` without a stated reason                                                  | Implemented |
| `KHI005`              | Type in an interop area that is not internal                                      | Planned     |
| [`KHI006`](khi006.md) | Native library that does not match its interop area                               | Implemented |
| [`KHI007`](khi007.md) | `bool` in an interop struct                                                       | Implemented |
| `KHI008`              | Raw pointer on the public surface                                                 | Planned     |

## Shipped rules `KHAOSXXX`

Diagnostics from the asset catalog and menu generators. None yet.

## Adding a rule

1. Add the `DiagnosticDescriptor` to `Descriptors`, with a `helpLinkUri`
   pointing at `docs/rules/<id>.md`.
2. Add the row to `AnalyzerReleases.Unshipped.md`, or the build fails (RS2000).
3. Write the page here: cause, why, reported, fix, not reported, suppressing.
4. Write the tests, including one that reproduces the shape the code really has
   **after** the source generators have run. A test compilation runs no
   generator, which is not what the compiler sees during a real build.
