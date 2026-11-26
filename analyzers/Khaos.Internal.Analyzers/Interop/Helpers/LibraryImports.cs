// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop.Helpers;

internal static class LibraryImports
{
    private const string EntryPoint = "EntryPoint";

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

    internal static bool TryGetEntryPoint(AttributeData libraryImport, out TypedConstant entryPoint)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in libraryImport.NamedArguments)
        {
            if (argument.Key == EntryPoint)
            {
                entryPoint = argument.Value;
                return true;
            }
        }

        entryPoint = default;
        return false;
    }

    internal static bool TryGetEntryPointArgument(AttributeData libraryImport, CancellationToken cancellationToken, [NotNullWhen(true)] out AttributeArgumentSyntax? argument)
    {
        if (libraryImport.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax { ArgumentList: { } arguments })
        {
            argument = null;
            return false;
        }

        argument = arguments.Arguments.FirstOrDefault(static candidate => candidate.NameEquals?.Name.Identifier.ValueText == EntryPoint);
        return argument is not null;
    }
}
