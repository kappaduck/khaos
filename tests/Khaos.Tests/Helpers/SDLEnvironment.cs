// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Helpers;

internal static partial class SDLEnvironment
{
    private const int Overwrite = 1;
    private const int Failed = -1;

    internal static void Set(string name, string value)
    {
        if (SetUnsafe(name, value, Overwrite) == Failed)
            throw new InvalidOperationException($"SDL could not set the environment variable {name} to '{value}'.");
    }

    internal static void Unset(string name)
    {
        if (UnsetUnsafe(name) == Failed)
            throw new InvalidOperationException($"SDL could not remove the environment variable {name}.");
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_setenv_unsafe", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static safe partial int SetUnsafe(string name, string value, int overwrite);

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_unsetenv_unsafe", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static safe partial int UnsetUnsafe(string name);
}
