// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.Win32.Primitives;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Interop.Win32;

[SupportedOSPlatform("windows")]
internal static partial class Kernel32
{
    internal const uint EnableVirtualTerminalProcessing = 0x0004;

    [LibraryImport(nameof(Kernel32), EntryPoint = "GetConsoleMode")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool GetConsoleMode(HANDLE* handle, out uint mode);

    [LibraryImport(nameof(Kernel32), EntryPoint = "SetConsoleMode")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static unsafe partial bool SetConsoleMode(HANDLE* handle, uint mode);
}
