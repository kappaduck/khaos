// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Interop.SDL;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers;

internal static partial class SDLKeyboard
{
    internal static string ScancodeName(Scancode code)
    {
        unsafe
        {
            return Marshal.PtrToStringUTF8((nint)GetScancodeName((int)code)) ?? string.Empty;
        }
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetScancodeName")]
    private static unsafe partial byte* GetScancodeName(int scancode);
}
