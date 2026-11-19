// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos.Tools.Markdown;

internal sealed class Changelog
{
    private const string ChangedHeading = "### Changed";
    private const string NativeLinePrefix = "- `KappaDuck.Khaos.Native` ";
    private const string Separator = "&#8212;";
    private const string SectionPrefix = "## ";
    private const string UnreleasedMarker = "[Unreleased]";

    private readonly string _path;
    private string[] _lines;

    private Changelog(string path, string[] lines)
    {
        _lines = lines;
        _path = path;
    }

    internal static async Task<Changelog> ParseAsync(string path)
    {
        string[] lines = await File.ReadAllLinesAsync(path);
        return new Changelog(path, lines);
    }

    internal void OpenNext(Version current, Version next, int year)
    {
        if (Array.Exists(_lines, static l => IsUnreleasedHeading(l)))
            throw new InvalidOperationException($"'{_path}' already has an {UnreleasedMarker} section.");

        if (!HasSection(current))
            throw new InvalidOperationException($"'{_path}' has no section for {current}. Prepare and publish the release first.");

        int firstSection = Array.FindIndex(_lines, static l => l.StartsWith(SectionPrefix, StringComparison.Ordinal));
        string placeholder = string.Create(CultureInfo.InvariantCulture, $"{SectionPrefix}{next} {Separator} {year}-xx-xx {UnreleasedMarker}");

        _lines = [.. _lines.AsSpan(..firstSection), placeholder, string.Empty, .. _lines.AsSpan(firstSection..)];
    }

    internal void Release(Version version, DateOnly date)
    {
        int unreleased = Array.FindIndex(_lines, static l => IsUnreleasedHeading(l));

        if (unreleased < 0)
            throw new InvalidOperationException($"'{_path}' has no [Unreleased] section.");

        if (HasSection(version))
            throw new InvalidOperationException($"'{_path}' already has a section for {version}.");

        if (IsSectionEmpty(unreleased))
            throw new InvalidOperationException($"The {UnreleasedMarker} section of '{_path}' is empty.");

        _lines[unreleased] = string.Create(CultureInfo.InvariantCulture, $"{SectionPrefix}{version} {Separator} {date:yyyy-MM-dd}");
    }

    internal void SetNative(Version native, params ReadOnlySpan<KeyValuePair<string, Version>> libraries)
    {
        int unreleased = Array.FindIndex(_lines, static l => IsUnreleasedHeading(l));

        if (unreleased < 0)
            throw new InvalidOperationException($"'{_path}' has no {UnreleasedMarker} section. Open it with 'dotnet tools/release/next.cs' first.");

        int end = SectionEnd(unreleased);
        string line = CreateNativeLine(native, libraries);

        int startIndex = unreleased + 1;
        int count = end - unreleased - 1;

        int existing = Array.FindIndex(_lines, startIndex, count, static l => l.StartsWith(NativeLinePrefix, StringComparison.Ordinal));

        if (existing >= 0)
        {
            _lines[existing] = line;
            return;
        }

        int changed = Array.FindIndex(_lines, startIndex, count, static l => l.TrimEnd() == ChangedHeading);

        if (changed >= 0)
        {
            AddToChanged(changed, end, line);
            return;
        }

        int at = LastLineOf(unreleased, end) + 1;

        string[] section = [string.Empty, ChangedHeading, string.Empty, line];
        _lines = [.. _lines.AsSpan(..at), .. section, .. _lines.AsSpan(at..)];
    }

    public Task SaveAsync() => File.WriteAllTextAsync(_path, string.Join('\n', _lines) + '\n');

    private bool HasSection(Version version) => Array.Exists(_lines, l => IsVersionHeading(l, version));

    private static bool IsVersionHeading(ReadOnlySpan<char> line, Version version)
        => line.StartsWith($"{SectionPrefix}{version} ", StringComparison.Ordinal) && !IsUnreleasedHeading(line);

    private static bool IsUnreleasedHeading(ReadOnlySpan<char> line)
        => line.StartsWith(SectionPrefix, StringComparison.Ordinal) && line.TrimEnd().EndsWith(UnreleasedMarker, StringComparison.Ordinal);

    private bool IsSectionEmpty(int heading) => LastLineOf(heading, SectionEnd(heading)) == heading;

    private int LastLineOf(int heading, int end)
    {
        int last = end - 1;

        while (last > heading && string.IsNullOrWhiteSpace(_lines[last]))
            last--;

        return last;
    }

    private int SectionEnd(int heading)
    {
        int next = Array.FindIndex(_lines, heading + 1, static l => l.StartsWith(SectionPrefix, StringComparison.Ordinal));
        return next < 0 ? _lines.Length : next;
    }

    private static string CreateNativeLine(Version native, ReadOnlySpan<KeyValuePair<string, Version>> libraries)
    {
        if (libraries.IsEmpty)
            return string.Create(CultureInfo.InvariantCulture, $"{NativeLinePrefix}{native}");

        string[] names = new string[libraries.Length];

        for (int i = 0; i < libraries.Length; i++)
            names[i] = string.Create(CultureInfo.InvariantCulture, $"{libraries[i].Key} {libraries[i].Value}");

        string list = names.Length == 1 ? names[0] : $"{string.Join(", ", names[..^1])} and {names[^1]}";
        return string.Create(CultureInfo.InvariantCulture, $"{NativeLinePrefix}{native}, with {list}");
    }

    private void AddToChanged(int heading, int end, string line)
    {
        int at = heading + 1;

        while (at < end && string.IsNullOrWhiteSpace(_lines[at]))
            at++;

        if (at == end || !_lines[at].StartsWith("- ", StringComparison.Ordinal))
        {
            _lines = [.. _lines.AsSpan(..(heading + 1)), string.Empty, line, .. _lines.AsSpan((heading + 1)..)];
            return;
        }

        while (at < end && (_lines[at].StartsWith("- ", StringComparison.Ordinal) || _lines[at].StartsWith("  ", StringComparison.Ordinal)))
            at++;

        _lines = [.. _lines.AsSpan(..at), line, .. _lines.AsSpan(at..)];
    }
}
