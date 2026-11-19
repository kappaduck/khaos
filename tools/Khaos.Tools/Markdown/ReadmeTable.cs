// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Markdown;

public abstract class ReadmeTable
{
    private readonly string[] _lines;
    private readonly string _newLine;
    private readonly int _start;
    private readonly int _length;

    protected ReadmeTable(string path, string heading, string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        _newLine = content.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        string[] lines = content.Split(_newLine);

        int headingIndex = Array.FindIndex(lines, l => l.AsSpan().Trim().SequenceEqual(heading));
        int start = headingIndex < 0 ? -1 : Array.FindIndex(lines, headingIndex + 1, static l => l.AsSpan().TrimStart().StartsWith('|'));

        if (start < 0)
            throw new FormatException($"Cannot find the table under '{heading}' in '{path}'.");

        int end = start;

        while (end < lines.Length && lines[end].AsSpan().TrimStart().StartsWith('|'))
            end++;

        _lines = lines;
        _start = start;
        _length = end - start;

        Path = path;
        Table = MarkdownTable.Parse(lines.AsSpan(start..end));
    }

    protected string Path { get; }

    protected MarkdownTable Table { get; }

    public Task SaveAsync()
    {
        string[] lines = [.. _lines.AsSpan(.._start), .. Table.Render(), .. _lines.AsSpan((_start + _length)..)];
        return File.WriteAllTextAsync(Path, string.Join(_newLine, lines));
    }

    protected static string Code(Version version) => $"`{version}`";
}
