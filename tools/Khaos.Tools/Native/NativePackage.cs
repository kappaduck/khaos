// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Native;

internal sealed class NativePackage
{
    private const string OpenTag = "<version>";
    private const string CloseTag = "</version>";

    private readonly string _path;
    private readonly int _versionStart;

    private string _content;
    private int _versionLength;

    private NativePackage(string path, string content, int start, int length, Version version)
    {
        _path = path;
        _content = content;
        _versionStart = start;
        _versionLength = length;

        Version = version;
    }

    public Version Version { get; private set; }

    public static async Task<NativePackage> ParseAsync(string path)
    {
        string content = await File.ReadAllTextAsync(path);

        int open = content.IndexOf(OpenTag, StringComparison.Ordinal);
        int close = open < 0 ? -1 : content.IndexOf(CloseTag, open, StringComparison.Ordinal);

        if (close < 0)
            throw new FormatException($"Cannot find the package version in '{path}'.");

        int start = open + OpenTag.Length;
        int length = close - start;

        ReadOnlySpan<char> text = content.AsSpan(start, length).Trim();

        if (!Version.TryParse(text, out Version? version) || version.Build < 0)
            throw new FormatException($"'{text}' in '{path}' is not a major.minor.patch version.");

        return new(path, content, start, length, version);
    }

    public void Bump(IReadOnlyList<SDLUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        bool major = updates.Any(static u => u.Latest.Major != u.Current.Major);
        bool minor = updates.Any(static u => u.Latest.Minor != u.Current.Minor);

        Version next = (major, minor) switch
        {
            (true, _) => new Version(Version.Major + 1, 0, 0),
            (false, true) => new Version(Version.Major, Version.Minor + 1, 0),
            (false, false) => new Version(Version.Major, Version.Minor, Version.Build + 1),
        };

        string text = next.ToString();

        _content = string.Concat(_content.AsSpan(0, _versionStart), text, _content.AsSpan(_versionStart + _versionLength));
        _versionLength = text.Length;

        Version = next;
    }

    public Task SaveAsync() => File.WriteAllTextAsync(_path, _content);
}
