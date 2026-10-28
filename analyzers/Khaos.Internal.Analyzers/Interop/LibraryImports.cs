// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KappaDuck.Khaos.Internal.Analyzers.Interop;

internal static class LibraryImports
{
    internal static INamedTypeSymbol? AttributeType(Compilation compilation)
        => compilation.GetTypeByMetadataName("System.Runtime.InteropServices.LibraryImportAttribute");

    internal static AttributeData? Find(IMethodSymbol method, INamedTypeSymbol libraryImport)
        => method.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, libraryImport));

    internal static MethodDeclarationSyntax? DeclarationOf(AttributeData libraryImport, CancellationToken cancellationToken)
        => libraryImport.ApplicationSyntaxReference?.GetSyntax(cancellationToken).Parent?.Parent as MethodDeclarationSyntax;
}
