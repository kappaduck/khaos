// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class Fields
{
    internal static bool TryGetCapturedParameter(IFieldSymbol field, [NotNullWhen(true)] out IParameterSymbol? parameter)
    {
        parameter = field.ContainingType.InstanceConstructors
            .SelectMany(static constructor => constructor.Parameters)
            .FirstOrDefault(candidate => candidate.Locations.SequenceEqual(field.Locations));

        return parameter is not null;
    }
}
