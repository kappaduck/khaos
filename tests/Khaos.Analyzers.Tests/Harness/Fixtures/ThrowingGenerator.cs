// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

internal sealed class ThrowingGenerator : IIncrementalGenerator
{
    internal const string Message = "The generator fixture failed on purpose.";

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterSourceOutput(context.CompilationProvider, static (_, _) => throw new InvalidOperationException(Message));
}
