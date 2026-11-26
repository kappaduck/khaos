// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;
using Microsoft.CodeAnalysis.Diagnostics;

namespace KappaDuck.Khaos.Analyzers.Tests;

internal sealed class DescriptorsTests
{
    private const string Repository = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";

    private static readonly DiagnosticAnalyzer[] _internalAnalyzers =
    [
        new InteropBoolMarshallingAnalyzer()
    ];

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
}
