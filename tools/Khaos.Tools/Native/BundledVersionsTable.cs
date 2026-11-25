// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;

namespace KappaDuck.Khaos.Tools.Native;

internal sealed class BundledVersionsTable : ReadmeTable
{
    private BundledVersionsTable(string path, string content) : base(path, "## Bundled versions", content)
    {
    }

    public static async Task<BundledVersionsTable> ParseAsync(string path)
        => new(path, await File.ReadAllTextAsync(path));

    internal void Update(IReadOnlyDictionary<string, Version> bundled)
    {
        for (int row = 0; row < bundled.Count; row++)
        {
            if (bundled.TryGetValue(Table[row, 0], out Version? version))
                Table[row, 1] = Code(version);
        }
    }
}
