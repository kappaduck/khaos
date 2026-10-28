// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace KappaDuck.Khaos.Testing.Harness;

internal sealed class AnalyzerOptionsProvider(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptionsProvider
{
    private readonly Options _options = new(options);

    public override AnalyzerConfigOptions GlobalOptions => _options;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _options;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _options;

    private sealed class Options(IReadOnlyDictionary<string, string> options) : AnalyzerConfigOptions
    {
        private readonly ImmutableDictionary<string, string> _options = options.ToImmutableDictionary(KeyComparer);

        public override IEnumerable<string> Keys => _options.Keys;

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _options.TryGetValue(key, out value);
    }
}
