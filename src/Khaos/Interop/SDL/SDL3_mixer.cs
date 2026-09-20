// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3_mixer
{
    [LibraryImport(nameof(SDL3_mixer), EntryPoint = "MIX_Version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static safe partial int Version();
}
