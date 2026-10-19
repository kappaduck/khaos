// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics;

namespace Khaos.Utils;

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

    public int ColumnCount => _header.Length;

    public int RowCount => _rows.Count;

    public ReadOnlySpan<string> this[int row] => _rows[row];

    public string this[int row, int column]
    {
        get => _rows[row][column];
        set
        {
            EnsureValidCell(value);
            _rows[row][column] = value;
        }
    }

    public static MarkdownTable Parse(ReadOnlySpan<string> lines)
    {
        if (lines.Length < 2)
            throw new FormatException("A markdown table needs a header and a separator row.");

        string[] header = SplitRow(lines[0], CountCells(lines[0]));
        ColumnAlignment[] alignments = ParseAlignments(lines[1]);

        if (alignments.Length != header.Length)
            throw new FormatException($"The separator row has {alignments.Length} cells, but the header has {header.Length}.");

        ReadOnlySpan<string> body = lines[2..];
        List<string[]> rows = [with(body.Length)];

        foreach (string line in body)
            rows.Add(SplitRow(line, header.Length));

        return new MarkdownTable(header, alignments, rows);
    }

    public int IndexOfColumn(string name) => Array.IndexOf(_header, name);

    public int IndexOfRow(int column, string value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, _header.Length);

        for (int row = 0; row < _rows.Count; row++)
        {
            if (_rows[row][column] == value)
                return row;
        }

        return -1;
    }

    public void Insert(int index, params ReadOnlySpan<string> cells) => _rows.Insert(index, CreateRow(cells));

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

    public void Replace(int index, params ReadOnlySpan<string> cells) => _rows[index] = CreateRow(cells);

    private string[] CreateRow(ReadOnlySpan<string> cells)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(cells.Length, _header.Length);

        foreach (string cell in cells)
            EnsureValidCell(cell);

        return cells.ToArray();
    }

    private static void EnsureValidCell(ReadOnlySpan<char> cell)
    {
        if (cell.ContainsAny('|', '\r', '\n'))
            throw new ArgumentException($"A cell cannot contain '|' or a line break: '{cell}'.", nameof(cell));
    }

    private static string[] SplitRow(ReadOnlySpan<char> line, int columns)
    {
        ReadOnlySpan<char> content = TrimPipes(line);
        string[] cells = new string[columns];
        int column = 0;

        foreach (Range range in content.Split('|'))
        {
            if (column == columns)
                break;

            cells[column++] = content[range].Trim().ToString();
        }

        cells.AsSpan(column).Fill(string.Empty);
        return cells;
    }

    private static ColumnAlignment[] ParseAlignments(ReadOnlySpan<char> line)
    {
        ReadOnlySpan<char> content = TrimPipes(line);
        ColumnAlignment[] alignments = new ColumnAlignment[CountCells(line)];
        int column = 0;

        foreach (Range range in content.Split('|'))
            alignments[column++] = ParseAlignment(content[range].Trim());

        return alignments;
    }

    private static ColumnAlignment ParseAlignment(ReadOnlySpan<char> cell)
    {
        ReadOnlySpan<char> dashes = cell.Trim(':');

        if (dashes.IsEmpty || dashes.ContainsAnyExcept('-'))
            throw new FormatException($"'{cell}' is not a valid separator cell.");

        return (cell[0] == ':', cell[^1] == ':') switch
        {
            (true, true) => ColumnAlignment.Center,
            (true, false) => ColumnAlignment.Left,
            (false, true) => ColumnAlignment.Right,
            (false, false) => ColumnAlignment.None,
        };
    }

    private static ReadOnlySpan<char> TrimPipes(ReadOnlySpan<char> line)
    {
        ReadOnlySpan<char> trimmed = line.Trim();

        if (trimmed.StartsWith('|'))
            trimmed = trimmed[1..];

        if (trimmed.EndsWith('|'))
            trimmed = trimmed[..^1];

        return trimmed;
    }

    private static int CountCells(ReadOnlySpan<char> line) => TrimPipes(line).Count('|') + 1;

    private string RenderRow(ReadOnlySpan<string> cells, ReadOnlySpan<int> widths)
    {
        int length = LineLength(widths);
        Span<char> line = stackalloc char[length];
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
            position += widths[column] + 3;
        }

        line[position] = '|';
        return new string(line);
    }

    private string RenderSeparator(ReadOnlySpan<int> widths)
    {
        int length = LineLength(widths);
        Span<char> line = stackalloc char[length];
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

            position += widths[column] + 3;
        }

        line[position] = '|';
        return new string(line);
    }

    private static int LineLength(ReadOnlySpan<int> widths)
    {
        int length = 1;

        foreach (int width in widths)
            length += width + 3;

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
