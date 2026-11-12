// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal const string MainCallbackRate = "SDL_MAIN_CALLBACK_RATE";
    internal const string MainCallbackRateWaitEvent = "waitevent";
    internal const string NoSignalHandlers = "SDL_NO_SIGNAL_HANDLERS";
    internal const string RenderDriver = "SDL_RENDER_DRIVER";
    internal const string VideoDriver = "SDL_VIDEO_DRIVER";

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ResetHint", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool ResetHint(string name);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetHint", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool SetHint(string name, string? value);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetHintWithPriority", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static safe partial bool SetHintWithPriority(string name, string? value, SDL_HintPriority priority);
}
