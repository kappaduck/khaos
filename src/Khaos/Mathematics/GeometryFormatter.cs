// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;
using System.Text;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Formats the components of a geometry type as <c>(a, b, ...)</c>.
/// </summary>
/// <remarks>
/// The numeric format and the provider are applied to every component. The components are separated by
/// <c>"; "</c> instead of <c>", "</c> when a comma could be read as part of a number: when the provider uses a
/// comma as its decimal separator (<c>fr-CA</c>, <c>de-DE</c>, ...), or when a formatted component contains a
/// comma (<c>N0</c> in the invariant culture writes <c>1,000</c>). Otherwise <c>(1,5, 2,5)</c> or
/// <c>(1,000, 2,000)</c> would be ambiguous.
/// </remarks>
internal static class GeometryFormatter
{
    private const string CommaSeparator = ", ";
    private const string SemicolonSeparator = "; ";
    private const int StackSeparators = 8;

    internal static string ToString<T>(in T value, string? format, IFormatProvider? provider) where T : ISpanFormattable
    {
        DefaultInterpolatedStringHandler handler = new(0, 1, provider);
        handler.AppendFormatted(value, format);

        return handler.ToStringAndClear();
    }

    internal static bool TryFormat<T>(Span<char> destination, out int charsWritten, ReadOnlySpan<T> components, ReadOnlySpan<char> format, IFormatProvider? provider)
        where T : ISpanFormattable
    {
        string separator = Separator(provider);
        Span<int> separatorStarts = components.Length <= StackSeparators ? stackalloc int[StackSeparators] : new int[components.Length];
        bool componentHasComma = false;
        int written = 0;

        if (!TryAppend(destination, ref written, "("))
            return Fail(out charsWritten);

        for (int i = 0; i < components.Length; i++)
        {
            if (i > 0)
            {
                separatorStarts[i - 1] = written;

                if (!TryAppend(destination, ref written, separator))
                    return Fail(out charsWritten);
            }

            if (!components[i].TryFormat(destination[written..], out int count, format, provider))
                return Fail(out charsWritten);

            componentHasComma |= destination.Slice(written, count).Contains(',');
            written += count;
        }

        if (!TryAppend(destination, ref written, ")"))
            return Fail(out charsWritten);

        if (componentHasComma)
            UseSemicolons(destination, separatorStarts[..Math.Max(components.Length - 1, 0)]);

        charsWritten = written;
        return true;
    }

    internal static bool TryFormat<T>(Span<byte> destination, out int bytesWritten, ReadOnlySpan<T> components, ReadOnlySpan<char> format, IFormatProvider? provider)
        where T : IUtf8SpanFormattable
    {
        string separator = Separator(provider);
        Span<int> separatorStarts = components.Length <= StackSeparators ? stackalloc int[StackSeparators] : new int[components.Length];
        bool componentHasComma = false;
        int written = 0;

        if (!TryAppend(destination, ref written, "("u8))
            return Fail(out bytesWritten);

        for (int i = 0; i < components.Length; i++)
        {
            if (i > 0)
            {
                separatorStarts[i - 1] = written;

                if (!TryAppendUtf8(destination, ref written, separator))
                    return Fail(out bytesWritten);
            }

            if (!components[i].TryFormat(destination[written..], out int count, format, provider))
                return Fail(out bytesWritten);

            componentHasComma |= destination.Slice(written, count).Contains((byte)',');
            written += count;
        }

        if (!TryAppend(destination, ref written, ")"u8))
            return Fail(out bytesWritten);

        if (componentHasComma)
            UseSemicolons(destination, separatorStarts[..Math.Max(components.Length - 1, 0)]);

        bytesWritten = written;
        return true;
    }

    private static bool Fail(out int written)
    {
        written = 0;
        return false;
    }

    private static string Separator(IFormatProvider? provider)
        => NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator == "," ? SemicolonSeparator : CommaSeparator;

    private static bool TryAppend(Span<char> destination, ref int written, ReadOnlySpan<char> value)
    {
        if (!value.TryCopyTo(destination[written..]))
            return false;

        written += value.Length;
        return true;
    }

    private static bool TryAppend(Span<byte> destination, ref int written, ReadOnlySpan<byte> value)
    {
        if (!value.TryCopyTo(destination[written..]))
            return false;

        written += value.Length;
        return true;
    }

    private static bool TryAppendUtf8(Span<byte> destination, ref int written, ReadOnlySpan<char> value)
    {
        if (!Encoding.UTF8.TryGetBytes(value, destination[written..], out int count))
            return false;

        written += count;
        return true;
    }

    private static void UseSemicolons(Span<char> destination, ReadOnlySpan<int> separatorStarts)
    {
        foreach (int start in separatorStarts)
            destination[start] = ';';
    }

    private static void UseSemicolons(Span<byte> destination, ReadOnlySpan<int> separatorStarts)
    {
        foreach (int start in separatorStarts)
            destination[start] = (byte)';';
    }
}
