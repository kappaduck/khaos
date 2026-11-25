// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Tools.Tests;

internal static class SetupVerify
{
    [ModuleInitializer]
    internal static void Initialize() => UseSourceFileRelativeDirectory("snapshots");
}
