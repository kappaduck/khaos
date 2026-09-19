; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
KHI001 | Interop | Error | bool does not follow the marshalling convention of its interop area, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi001.md)
KHI002 | Interop | Error | nint or IntPtr in an interop declaration, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi002.md)
KHI003 | Interop | Error | LibraryImport without an explicit EntryPoint, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi003.md)
KHI006 | Interop | Error | native library does not match its interop area, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi006.md)
KHI007 | Interop | Error | bool in an interop struct, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi007.md)
