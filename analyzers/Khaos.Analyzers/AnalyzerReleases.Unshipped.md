; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
KHAOS001 | Events | Error | [CustomEvents] is not on a readonly partial struct, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khaos001.md)
KHAOS002 | Events | Error | [CustomEvents] group has no events, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khaos002.md)
KHAOS003 | Events | Error | event in a [CustomEvents] group has the wrong shape, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khaos003.md)
KHAOS004 | Events | Warning | [CustomEvents] group is named like a member of CustomEvent, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khaos004.md)
KHAOS005 | Events | Error | event in a [CustomEvents] group is listed twice or shares a name, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khaos005.md)
KHAOS006 | Hosting | Warning | a game takes its window as a constructor parameter, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khaos006.md)
