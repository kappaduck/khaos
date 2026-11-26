// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Analyzers.Tests.Fixtures;

internal static class InteropSources
{
    internal static string SDL(string members) => $$"""
        using System;
        using System.Runtime.InteropServices;

        namespace KappaDuck.Khaos.Interop.SDL;

        internal static partial class SDL3
        {
        {{members}}
        }
        """;

    internal static string Win32(string members) => $$"""
        using System;
        using System.Runtime.InteropServices;

        namespace KappaDuck.Khaos.Interop.Win32;

        internal static partial class User32
        {
        {{members}}
        }
        """;
}
