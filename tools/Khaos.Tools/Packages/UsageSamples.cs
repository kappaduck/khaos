// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Text.RegularExpressions;

namespace KappaDuck.Khaos.Tools.Packages;

internal static partial class UsageSamples
{
    internal static IReadOnlyList<UsageSample>? Parse(string readme)
    {
        ArgumentNullException.ThrowIfNull(readme);

        string text = readme.ReplaceLineEndings("\n");
        int start = text.IndexOf("\n## Usage\n", StringComparison.Ordinal);

        if (start < 0)
            return null;

        int end = text.IndexOf("\n## ", start + 1, StringComparison.Ordinal);
        string usage = end < 0 ? text[start..] : text[start..end];

        MatchCollection blocks = CSharpBlock.Matches(usage);
        UsageSample[] samples = new UsageSample[blocks.Count];

        for (int i = 0; i < blocks.Count; i++)
        {
            string code = blocks[i].Groups["code"].Value;
            Match path = SamplePath.Match(code);

            if (!path.Success)
                return null;

            samples[i] = new UsageSample(path.Groups["path"].Value, code);
        }

        return samples.Length == 0 ? null : samples;
    }

    [GeneratedRegex("^```csharp\n(?<code>.*?)^```", RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline, 1000)]
    private static partial Regex CSharpBlock { get; }

    [GeneratedRegex(@"\A// (?<path>[A-Za-z0-9_.]+(/[A-Za-z0-9_.]+)+\.cs)\b", RegexOptions.CultureInvariant, 1000)]
    private static partial Regex SamplePath { get; }
}
