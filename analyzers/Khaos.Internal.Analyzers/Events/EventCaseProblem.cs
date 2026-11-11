// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

internal sealed class EventCaseProblem(DiagnosticDescriptor descriptor, params object[] arguments)
{
    internal object[] Arguments { get; } = arguments;

    internal DiagnosticDescriptor Descriptor { get; } = descriptor;
}
