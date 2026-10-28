// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class LibraryImports
{
    private const string EntryPoint = "EntryPoint";

    internal static INamedTypeSymbol? AttributeType(Compilation compilation)
        => compilation.GetTypeByMetadataName("System.Runtime.InteropServices.LibraryImportAttribute");

    internal static AttributeData? Find(IMethodSymbol method, INamedTypeSymbol libraryImport)
        => method.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, libraryImport));

    internal static TypedConstant? EntryPointOf(AttributeData libraryImport)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in libraryImport.NamedArguments)
        {
            if (argument.Key == EntryPoint)
                return argument.Value;
        }

        return null;
    }

    internal static AttributeArgumentSyntax? EntryPointArgumentOf(AttributeData libraryImport, CancellationToken cancellationToken)
    {
        if (libraryImport.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax { ArgumentList: { } arguments })
            return null;

        return arguments.Arguments.FirstOrDefault(static argument => argument.NameEquals?.Name.Identifier.ValueText == EntryPoint);
    }

    internal static MethodDeclarationSyntax? DeclarationOf(AttributeData libraryImport, CancellationToken cancellationToken)
        => libraryImport.ApplicationSyntaxReference?.GetSyntax(cancellationToken).Parent?.Parent as MethodDeclarationSyntax;
}
