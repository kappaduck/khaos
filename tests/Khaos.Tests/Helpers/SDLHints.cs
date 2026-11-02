// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers;

internal static partial class SDLHints
{
    private const string VideoDriver = "SDL_VIDEO_DRIVER";
    private const int Override = 2;

    internal static void ForceVideoDriver(string driver)
    {
        if (!SetHintWithPriority(VideoDriver, driver, Override))
            throw new InvalidOperationException($"SDL refused the video driver hint '{driver}'.");
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_SetHintWithPriority", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.U1)]
    private static safe partial bool SetHintWithPriority(string name, string value, int priority);
}
