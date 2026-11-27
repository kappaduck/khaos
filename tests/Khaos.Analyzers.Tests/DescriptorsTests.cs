// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;
using KappaDuck.Khaos.Analyzers.Events;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KappaDuck.Khaos.Analyzers.Tests;

internal sealed class DescriptorsTests
{
    private const string Repository = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string Site = "https://khaos.kappaduck.com/rules/";

    private static readonly DiagnosticAnalyzer[] _analyzers = GetAnalyzers(typeof(CustomEventsAnalyzer).Assembly);
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

    [Test]
    public async Task ErrorDescriptorsShouldNotBeConfigurable()
    {
        IEnumerable<string> ids = _analyzers
            .SelectMany(static analyzer => analyzer.SupportedDiagnostics)
            .Where(static descriptor => descriptor.DefaultSeverity == DiagnosticSeverity.Error && !descriptor.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable))
            .Select(static descriptor => descriptor.Id);

        string configurable = string.Join(", ", ids.Distinct().Order(StringComparer.Ordinal));

        await configurable.Should().BeEmpty();
    }

    [Test]
    public async Task WarningDescriptorsShouldStayConfigurable()
    {
        IEnumerable<string> ids = _analyzers
            .SelectMany(static analyzer => analyzer.SupportedDiagnostics)
            .Where(static descriptor => descriptor.DefaultSeverity == DiagnosticSeverity.Warning && descriptor.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable))
            .Select(static descriptor => descriptor.Id);

        string locked = string.Join(", ", ids.Distinct().Order(StringComparer.Ordinal));

        await locked.Should().BeEmpty();
    }

    [Test]
    public async Task HelpLinkShouldOpenThePageOfTheRuleOnTheSite()
    {
        IEnumerable<string> ids = _analyzers
            .SelectMany(static analyzer => analyzer.SupportedDiagnostics)
            .Where(static descriptor => !string.Equals(descriptor.HelpLinkUri, $"{Site}{descriptor.Id.ToLowerInvariant()}.html", StringComparison.Ordinal))
            .Select(static descriptor => descriptor.Id);

        string elsewhere = string.Join(", ", ids.Distinct().Order(StringComparer.Ordinal));

        await elsewhere.Should().BeEmpty();
    }

    [Test]
    public async Task EveryRuleShouldHaveItsPage()
    {
        IEnumerable<string> ids = _analyzers
            .SelectMany(static analyzer => analyzer.SupportedDiagnostics)
            .Where(static descriptor => !File.Exists(Path.Combine(AppContext.BaseDirectory, "rules", $"{descriptor.Id.ToLowerInvariant()}.md")))
            .Select(static descriptor => descriptor.Id);

        string missing = string.Join(", ", ids.Distinct().Order(StringComparer.Ordinal));

        await missing.Should().BeEmpty();
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
