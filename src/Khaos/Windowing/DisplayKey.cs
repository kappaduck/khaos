// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
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

    internal static int Pick(int rank, ReadOnlySpan<RectI> screens)
    {
        int rightmost = -1;

        for (int i = 0; i < screens.Length; i++)
        {
            if (Rank(screens[i], screens) == rank)
                return i;

            if (rightmost < 0 || LeftToRight(screens[i], screens[rightmost]) > 0)
                rightmost = i;
        }

        return rightmost;
    }

    internal static int Rank(RectI screen, ReadOnlySpan<RectI> screens)
    {
        int rank = 1;

        foreach (RectI other in screens)
        {
            if (LeftToRight(other, screen) < 0)
                rank++;
        }

        return rank;
    }

    private static int LeftToRight(RectI left, RectI right)
        => left.X != right.X ? left.X.CompareTo(right.X) : left.Y.CompareTo(right.Y);
}
