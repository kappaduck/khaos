// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics;

namespace KappaDuck.Khaos.Tools.Markdown;

public sealed class MarkdownTable
{
    private const int MinimumWidth = 3;

    private readonly string[] _header;
    private readonly ColumnAlignment[] _alignments;
    private readonly List<string[]> _rows;

    private MarkdownTable(string[] header, ColumnAlignment[] alignments, List<string[]> rows)
    {
        _header = header;
        _alignments = alignments;
        _rows = rows;
    }

    public ReadOnlySpan<string> Header => _header;

    public int RowCount => _rows.Count;

    public ReadOnlySpan<string> this[int row] => _rows[row];

    public string this[int row, int column]
    {
        get => _rows[row][column];
        set
        {
            ValidateCell(value);
            _rows[row][column] = value;
        }
    }

    internal static MarkdownTable Parse(ReadOnlySpan<string> lines)
    {
        if (lines.Length < 2)
            throw new FormatException("A markdown table needs a header and a separator row.");

        string[] header = SplitIntoCells(lines[0], CountCells(lines[0]));
        ColumnAlignment[] alignments = ParseSeparator(lines[1]);

        if (alignments.Length != header.Length)
            throw new FormatException($"The separator row has {alignments.Length} cells, but the header has {header.Length}.");

        ReadOnlySpan<string> body = lines[2..];
        List<string[]> rows = [with(body.Length)];

        foreach (string line in body)
            rows.Add(SplitIntoCells(line, header.Length));

        return new(header, alignments, rows);
    }

    public int ColumnIndexOf(string name) => Array.IndexOf(_header, name);

    public void Insert(int index, params ReadOnlySpan<string> cells) => _rows.Insert(index, ToRow(cells));

    public string[] Render()
    {
        Span<int> widths = stackalloc int[_header.Length];

        for (int column = 0; column < widths.Length; column++)
            widths[column] = Math.Max(MinimumWidth, _header[column].Length);

        foreach (string[] row in _rows)
        {
            for (int column = 0; column < widths.Length; column++)
                widths[column] = Math.Max(widths[column], row[column].Length);
        }

        string[] lines = new string[_rows.Count + 2];

        lines[0] = RenderRow(_header, widths);
        lines[1] = RenderSeparator(widths);

        for (int row = 0; row < _rows.Count; row++)
            lines[row + 2] = RenderRow(_rows[row], widths);

        return lines;
    }

    public void Replace(int index, params ReadOnlySpan<string> cells) => _rows[index] = ToRow(cells);

    public int RowIndexOf(int column, string value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, _header.Length);

        for (int row = 0; row < RowCount; row++)
        {
            if (_rows[row][column] == value)
                return row;
        }

        return -1;
    }

    private static string[] SplitIntoCells(ReadOnlySpan<char> line, int columns)
    {
        ReadOnlySpan<char> content = TrimPipes(line);
        string[] cells = new string[columns];

        int column = 0;
        foreach (Range cell in content.Split('|'))
        {
            if (column == columns)
                break;

            cells[column++] = content[cell].Trim().ToString();
        }

        cells.AsSpan(column).Fill(string.Empty);
        return cells;
    }

    private static ColumnAlignment[] ParseSeparator(ReadOnlySpan<char> separator)
    {
        ReadOnlySpan<char> content = TrimPipes(separator);
        ColumnAlignment[] alignments = new ColumnAlignment[CountCells(separator)];

        int column = 0;

        foreach (Range alignment in content.Split("|"))
            alignments[column++] = ParseAlignment(content[alignment].Trim());

        return alignments;
    }

    private static ColumnAlignment ParseAlignment(ReadOnlySpan<char> alignment)
    {
        ReadOnlySpan<char> dashes = alignment.Trim(":");

        if (dashes.IsEmpty || dashes.ContainsAnyExcept('-'))
            throw new FormatException($"'{alignment}' is not a valid separator cell.");

        return (alignment[0] is ':', alignment[^1] is ':') switch
        {
            (true, false) => ColumnAlignment.Left,
            (true, true) => ColumnAlignment.Center,
            (false, true) => ColumnAlignment.Right,
            (false, false) => ColumnAlignment.None
        };
    }

    private static int CountCells(ReadOnlySpan<char> line) => TrimPipes(line).Count('|') + 1;

    private static ReadOnlySpan<char> TrimPipes(ReadOnlySpan<char> line) => line.Trim('|');

    private string[] ToRow(ReadOnlySpan<string> cells)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(cells.Length, _header.Length);

        foreach (string cell in cells)
            ValidateCell(cell);

        return [.. cells];
    }

    private static void ValidateCell(ReadOnlySpan<char> cell)
    {
        if (cell.ContainsAny('|', '\r', '\n'))
            throw new ArgumentException($"A cell cannot contain '|' or a line break: '{cell}'.", nameof(cell));
    }

    private string RenderRow(ReadOnlySpan<string> cells, ReadOnlySpan<int> widths)
    {
        Span<char> line = stackalloc char[LineLength(widths)];
        line.Fill(' ');

        int position = 0;

        for (int column = 0; column < widths.Length; column++)
        {
            line[position] = '|';

            Span<char> cell = line.Slice(position + 2, widths[column]);
            ReadOnlySpan<char> text = cells[column];

            int padding = cell.Length - text.Length;

            int left = _alignments[column] switch
            {
                ColumnAlignment.None or ColumnAlignment.Left => 0,
                ColumnAlignment.Center => padding / 2,
                ColumnAlignment.Right => padding,
                _ => throw new UnreachableException(),
            };

            text.CopyTo(cell[left..]);
            position += widths[column] + MinimumWidth;
        }

        line[position] = '|';
        return line.ToString();
    }

    private string RenderSeparator(ReadOnlySpan<int> widths)
    {
        Span<char> line = stackalloc char[LineLength(widths)];
        line.Fill(' ');

        int position = 0;

        for (int column = 0; column < widths.Length; column++)
        {
            line[position] = '|';

            Span<char> cell = line.Slice(position + 2, widths[column]);
            cell.Fill('-');

            ColumnAlignment alignment = _alignments[column];

            if (alignment is ColumnAlignment.Left or ColumnAlignment.Center)
                cell[0] = ':';

            if (alignment is ColumnAlignment.Right or ColumnAlignment.Center)
                cell[^1] = ':';

            position += widths[column] + MinimumWidth;
        }

        line[position] = '|';
        return line.ToString();
    }

    private static int LineLength(ReadOnlySpan<int> widths)
    {
        int length = 1;

        foreach (int width in widths)
            length += width + MinimumWidth;

        return length;
    }

    private enum ColumnAlignment
    {
        None,
        Left,
        Center,
        Right,
    }
}
