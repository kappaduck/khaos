; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
KHI001 | Interop | Error | bool does not follow the marshalling convention of its interop area, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi001.md)
KHI009 | Interop | Error | bool in an interop struct, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi009.md)
