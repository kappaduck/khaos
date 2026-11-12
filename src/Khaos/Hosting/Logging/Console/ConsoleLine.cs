// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Globalization;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

internal static class ConsoleLine
{
    internal const int StackLength = 256;

    private const string DefaultColor = "\e[39m";
    private const string Spaces = "  ";
    private const int TagWidth = 5;

    internal static void Append(ref DefaultInterpolatedStringHandler text, in ConsoleEntry entry, string timestampFormat, bool colored)
    {
        if (entry.Time is { } now)
        {
            text.AppendFormatted(now, timestampFormat);
            text.AppendLiteral(" ");
        }

        string tag = Tag(entry.Level);

        if (colored)
            text.AppendLiteral(ColorOf(entry.Level));

        text.AppendLiteral("[");
        text.AppendLiteral(tag);
        text.AppendLiteral("]");

        if (colored)
            text.AppendLiteral(DefaultColor);

        text.AppendLiteral(":");
        text.AppendFormatted(Spaces.AsSpan(0, TagWidth - tag.Length + 1));
        text.AppendFormatted(entry.Message);

        if (entry.Exception is not null)
        {
            text.AppendLiteral(Environment.NewLine);
            text.AppendFormatted(entry.Exception);
        }
    }

    internal static string Format(in ConsoleEntry entry, string timestampFormat, bool colored)
    {
        DefaultInterpolatedStringHandler text = new(0, 0, CultureInfo.InvariantCulture, stackalloc char[StackLength]);
        Append(ref text, in entry, timestampFormat, colored);

        return text.ToStringAndClear();
    }

    internal static string Tag(LogLevel level) => level switch
    {
        LogLevel.Trace => "trace",
        LogLevel.Debug => "debug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        LogLevel.None => "none",
        _ => "none"
    };

    private static string ColorOf(LogLevel level) => level switch
    {
        LogLevel.Information => "\e[32m",
        LogLevel.Warning => "\e[93m",
        LogLevel.Error or LogLevel.Critical => "\e[91m",
        LogLevel.Trace or LogLevel.Debug or LogLevel.None => "\e[37m",
        _ => "\e[37m"
    };
}
