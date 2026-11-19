// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Releases;

internal sealed class VersionPrefix
{
    private const string OpenTag = "<VersionPrefix>";
    private const string CloseTag = "</VersionPrefix>";

    private readonly string _path;
    private string _content;

    private VersionPrefix(string path, string content, Version version)
    {
        _path = path;
        _content = content;

        Version = version;
    }

    internal Version Version { get; private set; }

    internal static async Task<VersionPrefix> ParseAsync(string path)
    {
        string content = await File.ReadAllTextAsync(path);

        (int start, int length) = GetVersionRange(content, path);
        ReadOnlySpan<char> text = content.AsSpan(start, length).Trim();

        if (!Version.TryParse(text, out Version? version) || version.Build < 0 || version.Revision >= 0)
            throw new FormatException($"'{text}' is not a major.minor.patch VersionPrefix in '{path}'.");

        return new(path, content, version);
    }

    internal void Raise(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (version <= Version)
            throw new ArgumentException($"The next version ({version}) must be greater than the current one ({Version})", nameof(version));

        (int start, int length) = GetVersionRange(_content, _path);

        _content = string.Concat(_content.AsSpan(0, start), version.ToString(), _content.AsSpan(start + length));
        Version = version;
    }

    public Task SaveAsync() => File.WriteAllTextAsync(_path, _content);

    private static (int Start, int Length) GetVersionRange(string content, string path)
    {
        int open = content.IndexOf(OpenTag, StringComparison.Ordinal);

        if (open < 0)
            throw new FormatException($"Cannot find a {OpenTag} element in '{path}'.");

        int close = content.IndexOf(CloseTag, open, StringComparison.Ordinal);

        int start = open + OpenTag.Length;
        return (start, close - start);
    }
}
