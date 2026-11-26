// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class LibraryImports
{
    internal static bool TryGetAttribute(Compilation compilation, [NotNullWhen(true)] out INamedTypeSymbol? attribute)
    {
        attribute = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.LibraryImportAttribute");
        return attribute is not null;
    }

    internal static bool TryGetAttributeData(IMethodSymbol method, INamedTypeSymbol libraryImport, [NotNullWhen(true)] out AttributeData? data)
    {
        if (method.PartialDefinitionPart is not null)
        {
            data = null;
            return false;
        }

        data = method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, libraryImport));
        return data is not null;
    }
}
