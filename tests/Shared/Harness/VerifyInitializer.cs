// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Testing.Harness;

internal static class VerifyInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        UseSourceFileRelativeDirectory("snapshots");
    }
}
