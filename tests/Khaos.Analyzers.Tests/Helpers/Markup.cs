// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace KappaDuck.Khaos.Analyzers.Tests.Helpers;

internal sealed record Markup(ImmutableArray<string> Sources, string Expected)
{
    private const string Close = "|}";
    private const string Open = "{|";

    internal static Markup Parse(params ReadOnlySpan<string> markups)
    {
        ImmutableArray<string>.Builder sources = ImmutableArray.CreateBuilder<string>(markups.Length);
        List<(string Path, int Start, string Id, string Text)> expected = [];

        for (int i = 0; i < markups.Length; i++)
        {
            (string source, List<(string Id, TextSpan Span)> marks) = Strip(markups[i]);

            TextLineCollection lines = SourceText.From(source).Lines;
            string path = TestCompilation.FileName(i);

            sources.Add(source);

            foreach ((string id, TextSpan span) in marks)
                expected.Add((path, span.Start, id, DiagnosticText.Locate(id, path, lines.GetLinePositionSpan(span))));
        }

        IEnumerable<string> ordered = expected
            .OrderBy(static mark => mark.Path, StringComparer.Ordinal)
            .ThenBy(static mark => mark.Start)
            .ThenBy(static mark => mark.Id, StringComparer.Ordinal)
            .Select(static mark => mark.Text);

        return new Markup(sources.MoveToImmutable(), string.Join(Environment.NewLine, ordered));
    }

    private static (string Source, List<(string Id, TextSpan Span)> Marks) Strip(string markup)
    {
        StringBuilder source = new(markup.Length);
        Stack<(string Id, int Start)> opened = [];
        List<(string Id, TextSpan Span)> marks = [];

        int index = 0;

        while (index < markup.Length)
        {
            ReadOnlySpan<char> rest = markup.AsSpan(index);

            if (rest.StartsWith(Open, StringComparison.Ordinal))
            {
                int colon = markup.IndexOf(':', index + Open.Length);

                if (colon < 0)
                    throw new ArgumentException($"The mark at {index} has no ':' after its id: write {{|KHI001:code|}}.", nameof(markup));

                opened.Push((markup[(index + Open.Length)..colon], source.Length));
                index = colon + 1;
            }
            else if (rest.StartsWith(Close, StringComparison.Ordinal))
            {
                if (!opened.TryPop(out (string Id, int Start) mark))
                    throw new ArgumentException($"The '|}}' at {index} closes no mark.", nameof(markup));

                marks.Add((mark.Id, TextSpan.FromBounds(mark.Start, source.Length)));
                index += Close.Length;
            }
            else
            {
                source.Append(markup[index]);
                index++;
            }
        }

        if (opened.TryPeek(out (string Id, int Start) unclosed))
            throw new ArgumentException($"The mark {unclosed.Id} is never closed with '|}}'.", nameof(markup));

        return (source.ToString(), marks);
    }
}
