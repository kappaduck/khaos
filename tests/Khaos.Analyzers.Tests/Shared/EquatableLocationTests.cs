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
    public async Task LocationsFromEquivalentTreesShouldBeEqual()
    {
        EquatableLocation? left = EquatableLocation.From(Declaration());
        EquatableLocation? right = EquatableLocation.From(Declaration());

        bool result = left.Equals(right);
        await result.Should().BeTrue();
    }

    [Test]
    public async Task RoslynLocationsFromEquivalentTreesShouldNotBeEqual()
    {
        Location left = Declaration().GetLocation();
        Location right = Declaration().GetLocation();

        bool result = left.Equals(right);
        await result.Should().BeFalse();
    }

    [Test]
    public async Task LocationdPreserveTheLineSpan()
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
        DiagnosticDescriptor descriptor = new("KHAOS0001", "Title", "Message", "Usage", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        Diagnostic diagnostic = EquatableLocation.From(Declaration()).ToDiagnostic(descriptor);

        FileLinePositionSpan line = diagnostic.Location.GetLineSpan();
        await line.Path.Should().BeEqualTo("Window.cs");
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
