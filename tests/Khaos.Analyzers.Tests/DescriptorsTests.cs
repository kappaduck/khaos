// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KappaDuck.Khaos.Analyzers.Tests;

internal sealed class DescriptorsTests
{
    private const string Repository = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";

    private static readonly DiagnosticAnalyzer[] _internalAnalyzers = GetAnalyzers(typeof(InteropBoolMarshallingAnalyzer).Assembly);

    [Test]
    public async Task EveryInternalRuleShouldHaveItsPage()
    {
        IEnumerable<string> ids = _internalAnalyzers
            .SelectMany(static analyzer => analyzer.SupportedDiagnostics)
            .Where(static descriptor => !File.Exists(Path.Combine(AppContext.BaseDirectory, "rules", $"{descriptor.Id.ToLowerInvariant()}.md")))
            .Select(static descriptor => descriptor.Id);

        string missing = string.Join(", ", ids.Distinct().Order(StringComparer.Ordinal));

        await missing.Should().BeEmpty();
    }

    [Test]
    public async Task InternalHelpLinkShouldOpenThePageOfTheRuleOnGitHub()
    {
        IEnumerable<string> ids = _internalAnalyzers
            .SelectMany(static analyzer => analyzer.SupportedDiagnostics)
            .Where(static descriptor => !string.Equals(descriptor.HelpLinkUri, $"{Repository}{descriptor.Id.ToLowerInvariant()}.md", StringComparison.Ordinal))
            .Select(static descriptor => descriptor.Id);

        string elsewhere = string.Join(", ", ids.Distinct().Order(StringComparer.Ordinal));

        await elsewhere.Should().BeEmpty();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Test-only reflection over the analyzer assemblies, which the tests are never trimmed with.")]
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Test-only reflection: every analyzer has the public parameterless constructor the compiler also calls.")]
    private static DiagnosticAnalyzer[] GetAnalyzers(Assembly assembly)
    {
        return
        [
            .. assembly.GetTypes()
                .Where(static t => t is { IsAbstract: false } && t.IsSubclassOf(typeof(DiagnosticAnalyzer)))
                .Select(static t => (DiagnosticAnalyzer)Activator.CreateInstance(t)!)
        ];
    }
}
