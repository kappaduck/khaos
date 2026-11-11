// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Analyzers.Events;

internal sealed class CustomEventProblem(DiagnosticDescriptor descriptor, Location location, params object[] arguments)
{
    internal object[] Arguments { get; } = arguments;

    internal DiagnosticDescriptor Descriptor { get; } = descriptor;

    internal Location Location { get; } = location;
}
