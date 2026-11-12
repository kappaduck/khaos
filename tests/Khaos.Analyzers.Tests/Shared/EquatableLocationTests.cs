// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared;

internal sealed class EquatableLocationTests
{
    private const string Source = """
        namespace KappaDuck.Khaos;

        public sealed class Window
        {
        }
        """;

    [Test]
    public async Task FromSyntaxNodeShouldCaptureTheFilePath()
    {
        EquatableLocation? location = EquatableLocation.From(Declaration());

        await location.FilePath.Should().BeEqualTo("Window.cs");
    }

    [Test]
    public async Task EqualsShouldReturnTrueForLocationsFromEquivalentTrees()
    {
        EquatableLocation? left = EquatableLocation.From(Declaration());
        EquatableLocation? right = EquatableLocation.From(Declaration());

        bool result = left.Equals(right);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task RoslynLocationEqualsShouldReturnFalseForLocationsFromEquivalentTrees()
    {
        Location left = Declaration().GetLocation();
        Location right = Declaration().GetLocation();

        bool result = left.Equals(right);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task ToLocationShouldPreserveTheLineSpan()
    {
        ClassDeclarationSyntax declaration = Declaration();

        Location original = declaration.GetLocation();
        Location restored = EquatableLocation.From(declaration).ToLocation();

        string line = restored.GetLineSpan().ToString();

        await line.Should().BeEqualTo(original.GetLineSpan().ToString());
    }

    [Test]
    public async Task FromLocationWithoutASourceTreeShouldReturnNull()
    {
        EquatableLocation? location = EquatableLocation.From(Location.None);

        await location.Should().BeNull();
    }

    [Test]
    public async Task ToDiagnosticShouldReportAtTheCapturedLocation()
    {
        DiagnosticDescriptor descriptor = new("KHAOS0001", "Title", "Message", "Usage", DiagnosticSeverity.Warning, true);

        Diagnostic diagnostic = EquatableLocation.From(Declaration()).ToDiagnostic(descriptor);

        FileLinePositionSpan line = diagnostic.Location.GetLineSpan();

        await line.Path.Should().BeEqualTo("Window.cs");
    }


    [Test]
    public async Task FromLocationShouldReturnNullForNoLocation()
    {
        EquatableLocation? location = EquatableLocation.From((Location?)null);

        await location.Should().BeNull();
    }

    [Test]
    public async Task FromSyntaxNodeShouldReturnNullForNoNode()
    {
        EquatableLocation? location = EquatableLocation.From((SyntaxNode?)null);

        await location.Should().BeNull();
    }

    [Test]
    public async Task FromSyntaxReferenceShouldReturnNullForNoReference()
    {
        EquatableLocation? location = EquatableLocation.From((SyntaxReference?)null);

        await location.Should().BeNull();
    }

    [Test]
    public async Task FromSyntaxReferenceShouldCaptureTheSpanOfTheDeclaration()
    {
        SyntaxReference reference = Window().DeclaringSyntaxReferences[0];

        EquatableLocation? location = EquatableLocation.From(reference);

        await location.Text.Should().BeEqualTo(reference.Span);
    }

    [Test]
    public async Task FromSymbolShouldCaptureItsSourceFile()
    {
        EquatableLocation? location = EquatableLocation.From(Window());

        string? path = location?.FilePath;

        await path.Should().BeEqualTo("Source0.cs");
    }

    private static INamedTypeSymbol Window()
    {
        return TestCompilation.Create(Source).GetTypeByMetadataName("KappaDuck.Khaos.Window")!;
    }

    private static ClassDeclarationSyntax Declaration()
    {
        return CSharpSyntaxTree.ParseText(Source, path: "Window.cs")
                               .GetRoot()
                               .DescendantNodes()
                               .OfType<ClassDeclarationSyntax>()
                               .First();
    }
}
