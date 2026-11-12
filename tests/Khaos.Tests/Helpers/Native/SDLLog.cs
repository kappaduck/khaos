// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers.Native;

internal static partial class SDLLog
{
    internal static nint DefaultOutputFunction() => (nint)SDL3.GetDefaultLogOutputFunction();

    internal static nint OutputFunction()
    {
        unsafe
        {
            delegate* unmanaged<void*, SDL_LogCategory, SDL_LogPriority, byte*, void> callback;
            void* userdata;

            GetLogOutputFunction(&callback, &userdata);
            return (nint)callback;
        }
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetLogPriority")]
    internal static safe partial SDL_LogPriority PriorityOf(SDL_LogCategory category);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_LogMessage", StringMarshalling = StringMarshalling.Utf8)]
    internal static safe partial void WriteWithoutFormatArguments(SDL_LogCategory category, SDL_LogPriority priority, string message);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetLogOutputFunction")]
    private static unsafe partial void GetLogOutputFunction(delegate* unmanaged<void*, SDL_LogCategory, SDL_LogPriority, byte*, void>* callback, void** userdata);
}
