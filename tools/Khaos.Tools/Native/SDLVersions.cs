// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Native;

internal sealed class SDLVersions
{
    private readonly string[] _lines;
    private readonly string _path;
    private readonly Dictionary<string, int> _entries = [];

    private SDLVersions(string path, string[] lines, Dictionary<string, int> entries)
    {
        _lines = lines;
        _path = path;
        _entries = entries;
    }

    public static async Task<SDLVersions> ParseAsync(string path)
    {
        string[] lines = await File.ReadAllLinesAsync(path);
        Dictionary<string, int> entries = [];

        for (int i = 0; i < lines.Length; i++)
        {
            ReadOnlySpan<char> line = lines[i].AsSpan().Trim();
            int separator = line.IndexOf('=');

            if (separator <= 0 || line.StartsWith('#'))
                continue;

            entries[line[..separator].Trim().ToString()] = i;
        }

        return new(path, lines, entries);
    }

    public Version GetVersion(string key)
    {
        int index = IndexOf(key);

        ReadOnlySpan<char> line = _lines[index];
        ReadOnlySpan<char> text = line[(line.IndexOf('=') + 1)..].Trim();

        if (!Version.TryParse(text, out Version? version))
            throw new FormatException($"'{text}' is not a valid {key} version in '{_path}'.");

        return version;
    }

    public Task SaveAsync() => File.WriteAllTextAsync(_path, string.Join('\n', _lines) + '\n');

    public void SetVersion(string key, Version version) => _lines[IndexOf(key)] = $"{key}={version}";

    private int IndexOf(string key)
        => _entries.TryGetValue(key, out int index) ? index : throw new FormatException($"'{_path}' has no {key} entry.");
}
