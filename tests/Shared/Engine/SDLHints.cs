// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Testing.Engine;

internal static partial class SDLHints
{
    private const string VideoDriver = "SDL_VIDEO_DRIVER";
    private const int Override = 2;

    internal static void ForceVideoDriver(string driver)
    {
        if (!SetHintWithPriority(VideoDriver, driver, Override))
            throw new InvalidOperationException($"SDL refused the video driver hint '{driver}'.");
    }

    internal static void Reset(string name)
    {
        if (!ResetHint(name))
            throw new InvalidOperationException($"SDL could not reset the hint {name}.");
    }

    internal static void Set(string name, string value)
    {
        if (!SetHint(name, value))
            throw new InvalidOperationException($"SDL refused the hint {name} = '{value}'.");
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_ResetHint", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static safe partial bool ResetHint(string name);

    [LibraryImport("SDL3", EntryPoint = "SDL_SetHint", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static safe partial bool SetHint(string name, string value);

    [LibraryImport("SDL3", EntryPoint = "SDL_SetHintWithPriority", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static safe partial bool SetHintWithPriority(string name, string value, int priority);
}
