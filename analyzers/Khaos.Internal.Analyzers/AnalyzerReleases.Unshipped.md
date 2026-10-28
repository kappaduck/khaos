; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
KHI001 | Interop | Error | bool does not follow the marshalling convention of its interop area, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi001.md)
KHI002 | Interop | Error | nint or IntPtr in an interop declaration, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi002.md)
KHI003 | Interop | Error | LibraryImport without an explicit EntryPoint, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi003.md)
KHI004 | Interop | Error | void* without a stated reason, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi004.md)
KHI005 | Interop | Error | type in an interop area is not internal, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi005.md)
KHI006 | Interop | Error | native library does not match its interop area, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi006.md)
KHI007 | Interop | Error | bool in an interop struct, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi007.md)
KHI008 | Interop | Error | raw pointer on the public surface, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi008.md)
KHI009 | Interop | Error | native import outside an interop area, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi009.md)
KHI010 | Interop | Error | bool or char in a native callback, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi010.md)
KHI011 | Interop | Error | char in Interop/SDL, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi011.md)
KHI012 | Interop | Error | non-copyable value used outside its using, [documentation](https://github.com/kappaduck/khaos/blob/main/docs/rules/khi012.md)
