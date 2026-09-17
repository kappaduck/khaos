// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests;

internal static class SnapshotSetup
{
    private const string Directory = "snapshots";

    [ModuleInitializer]
    internal static void Initialize()
    {
        VerifySourceGenerators.Initialize();

        DerivePathInfo((src, _, type, method) =>
        {
            string path = Path.Combine(Path.GetDirectoryName(src)!, Directory);
            return new PathInfo(path, type.Name, method.Name);
        });
    }
}
