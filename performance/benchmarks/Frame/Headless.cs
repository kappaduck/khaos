// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Benchmarks.Frame;

internal static partial class Headless
{
    private const string VideoDriver = "SDL_VIDEO_DRIVER";
    private const string Offscreen = "offscreen";
    private const int Override = 2;

    internal static void UseOffscreen()
    {
        if (!SetHintWithPriority(VideoDriver, Offscreen, Override))
            throw new InvalidOperationException("SDL refused the offscreen video driver hint.");
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_SetHintWithPriority", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static safe partial bool SetHintWithPriority(string name, string value, int priority);
}
