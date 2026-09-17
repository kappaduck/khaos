// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.InteropServices;
using System.Text;

namespace Utils;

public sealed class MarkdownTable
{
    private MarkdownTable(IReadOnlyList<string> header, IReadOnlyList<ColumnAlignment> alignments, IList<string[]> rows)
    {
        Header = header;
        Alignments = alignments;
        Rows = rows;
    }

    public IReadOnlyList<string> Header { get; }

    public IReadOnlyList<ColumnAlignment> Alignments { get; }

    public IList<string[]> Rows { get; }

    public static MarkdownTable Parse(string[] lines)
    {
        if (lines.Length < 2)
            throw new InvalidOperationException("A markdown table needs a header and a separator row.");

        string[] header = SplitRow(lines[0]);
        ColumnAlignment[] alignments = [.. SplitRow(lines[1]).Select(ParseAlignment)];
        List<string[]> rows = [.. lines.Skip(2).Select(SplitRow)];

        return new MarkdownTable(header, alignments, rows);
    }

    public Span<string> Render()
    {
        int[] widths = new int[Header.Count];

        for (int column = 0; column < widths.Length; column++)
            widths[column] = Math.Max(3, Rows.Select(r => r[column].Length).Append(Header[column].Length).Max());

        List<string> lines = [RenderRow(Header, widths), RenderRow([.. widths.Select((width, column) => Separator(Alignments[column], width))], widths)];
        lines.AddRange(Rows.Select(r => RenderRow(r, widths)));

        return CollectionsMarshal.AsSpan(lines);
    }

    private static string[] SplitRow(string line)
    {
        string trimmed = line.Trim();

        if (trimmed.StartsWith('|'))
            trimmed = trimmed[1..];

        if (trimmed.EndsWith('|'))
            trimmed = trimmed[..^1];

        return [.. trimmed.Split('|').Select(c => c.Trim())];
    }

    private static ColumnAlignment ParseAlignment(string cell) => (cell.StartsWith(':'), cell.EndsWith(':')) switch
    {
        (true, true) => ColumnAlignment.Center,
        (true, false) => ColumnAlignment.Left,
        (false, true) => ColumnAlignment.Right,
        _ => ColumnAlignment.None
    };

    private static string Separator(ColumnAlignment alignment, int width) => alignment switch
    {
        ColumnAlignment.Center => $":{new string('-', width - 2)}:",
        ColumnAlignment.Left => $":{new string('-', width - 1)}",
        ColumnAlignment.Right => $"{new string('-', width - 1)}:",
        _ => new string('-', width)
    };

    private string RenderRow(IReadOnlyList<string> cells, Span<int> widths)
    {
        StringBuilder builder = new("|");

        for (int column = 0; column < cells.Count; column++)
        {
            string cell = cells[column];
            int padding = widths[column] - cell.Length;

            int left = Alignments[column] switch
            {
                ColumnAlignment.Center => padding / 2,
                ColumnAlignment.Right => padding,
                _ => 0
            };

            builder.Append(' ', left + 1).Append(cell).Append(' ', padding - left + 1).Append('|');
        }

        return builder.ToString();
    }
}

public enum ColumnAlignment
{
    None,
    Left,
    Center,
    Right
}
