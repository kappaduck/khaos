// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;

namespace KappaDuck.Khaos.Tools.Native;

internal sealed class CompatibilityTable : ReadmeTable
{
    private const string Source = "`source`";

    private readonly int _khaosColumn;
    private readonly int _nativeColumn;

    private CompatibilityTable(string path, string content) : base(path, "## SDL compatibility", content)
    {
        _khaosColumn = Table.ColumnIndexOf("Khaos");
        _nativeColumn = Table.ColumnIndexOf("Native");

        if (_khaosColumn < 0 || _nativeColumn < 0)
            throw new FormatException($"The SDL compatibility table in '{path}' needs a Khaos and a Native column.");
    }

    internal static async Task<CompatibilityTable> ParseAsync(string path) => new(path, await File.ReadAllTextAsync(path));

    internal void AddRelease(Version khaos, Version native)
    {
        EnsureSourceRow();

        string khaosCell = Code(khaos);

        if (Table.RowIndexOf(_khaosColumn, khaosCell) >= 0)
            throw new InvalidOperationException($"The SDL compatibility table already has a row for Khaos {khaos}.");

        Version? sourceNative = ParseVersion(Table[0, _nativeColumn]);

        if (sourceNative != native)
            throw new InvalidOperationException($"Khaos references the native packages {native}, but the {Source} row of the SDL compatibility table ships {sourceNative?.ToString() ?? "an unknown version"}. Update one of them first.");

        string[] release = Table[0].ToArray();

        release[_khaosColumn] = khaosCell;
        release[_nativeColumn] = Code(native);

        Table.Insert(1, release);
    }

    internal IReadOnlyList<KeyValuePair<string, Version>> ChangedSinceRelease()
    {
        EnsureSourceRow();

        ReadOnlySpan<string> header = Table.Header;
        List<KeyValuePair<string, Version>> changed = [];

        for (int column = 0; column < header.Length; column++)
        {
            if (column == _khaosColumn || column == _nativeColumn)
                continue;

            string source = Table[0, column];

            if (Table.RowCount > 1 && Table[1, column] == source)
                continue;

            Version version = ParseVersion(source) ?? throw new FormatException($"'{source}' is not a version in the SDL compatibility table of '{Path}'.");
            changed.Add(new KeyValuePair<string, Version>(header[column], version));
        }

        return changed;
    }

    internal void SetSource(Version native, IReadOnlyDictionary<string, Version> bundled)
    {
        ArgumentNullException.ThrowIfNull(bundled);

        ReadOnlySpan<string> header = Table.Header;
        string[] row = new string[header.Length];

        for (int column = 0; column < header.Length; column++)
        {
            string name = header[column];

            row[column] = name switch
            {
                "Khaos" => Source,
                "Native" => Code(native),
                _ when bundled.TryGetValue(name, out Version? version) => Code(version),
                _ => throw new FormatException($"Unknown column '{name}' in the SDL compatibility table of '{Path}'."),
            };
        }

        if (Table.RowCount > 0 && Table[0, _khaosColumn] == Source)
        {
            Table.Replace(0, row);
            return;
        }

        Table.Insert(0, row);
    }

    private static Version? ParseVersion(string cell) => Version.TryParse(cell.AsSpan().Trim('`'), out Version? version) ? version : null;

    private void EnsureSourceRow()
    {
        if (Table.RowCount == 0 || Table[0, _khaosColumn] != Source)
            throw new FormatException($"The first row of the SDL compatibility table in '{Path}' must be the {Source} row.");
    }
}
