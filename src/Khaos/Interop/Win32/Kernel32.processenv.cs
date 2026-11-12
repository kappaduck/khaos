// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.Win32.Primitives;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Interop.Win32;

[SupportedOSPlatform("windows")]
internal static partial class Kernel32
{
    internal const nint InvalidHandleValue = -1;
    internal const uint StandardError = unchecked((uint)-12);
    internal const uint StandardOutput = unchecked((uint)-11);

    [LibraryImport(nameof(Kernel32), EntryPoint = "GetStdHandle")]
    internal static safe partial HANDLE* GetStdHandle(uint device);
}
