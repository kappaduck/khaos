// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos.Windowing;

internal static class DisplayKey
{
    private const string Separator = "#";

    internal static string Of(string name, int rank)
        => rank <= 1 ? name : string.Create(CultureInfo.InvariantCulture, $"{name} {Separator}{rank}");

    internal static (string Name, int Rank) Parse(string key)
    {
        int separator = key.LastIndexOf(Separator, StringComparison.Ordinal);

        if (separator > 0 && TryGetRank(key.AsSpan(separator + Separator.Length), out int rank))
            return (key[..(separator - Separator.Length)], rank);

        return (key, 1);

        static bool TryGetRank(ReadOnlySpan<char> key, out int rank)
        {
            return int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out rank)
                && rank > 1;
        }
    }
}
