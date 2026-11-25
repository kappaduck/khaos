// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Analyzers.Tests;

internal static class SetupVerify
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        VerifySourceGenerators.Initialize();
        UseSourceFileRelativeDirectory("snapshots");
    }
}
