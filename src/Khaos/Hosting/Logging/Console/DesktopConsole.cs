// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

[UnsupportedOSPlatform("browser")]
internal sealed class DesktopConsole : IConsole
{
    private readonly bool _colorsOnError;
    private readonly bool _colorsOnOutput;
    private readonly TextWriter _error;
    private readonly TextWriter _output;
    private readonly ConsoleQueue _queue;
    private readonly VirtualTerminal? _terminal;
    private readonly string _timestampFormat;

    internal DesktopConsole(ConsoleLogOptions options)
        : this(options, options.Colors == ConsoleColors.Never ? null : VirtualTerminal.Enable())
    {
    }

    internal DesktopConsole(ConsoleLogOptions options, TextWriter output, TextWriter error, bool colorsOnOutput, bool colorsOnError)
    {
        _output = output;
        _error = error;
        _colorsOnOutput = colorsOnOutput;
        _colorsOnError = colorsOnError;
        _timestampFormat = options.TimestampFormat;
        _queue = new ConsoleQueue(Print, options.MaxQueueLength, options.QueueFullMode);
    }

    private DesktopConsole(ConsoleLogOptions options, VirtualTerminal? terminal)
        : this(
            options,
            System.Console.Out,
            System.Console.Error,
            UsesColors(options.Colors, System.Console.IsOutputRedirected, terminal?.OnOutput ?? false),
            UsesColors(options.Colors, System.Console.IsErrorRedirected, terminal?.OnError ?? false)) => _terminal = terminal;

    public void Dispose()
    {
        _queue.Dispose();
        _terminal?.Dispose();
    }

    public void Write(in ConsoleEntry entry) => _queue.Enqueue(in entry);

    private void Print(ConsoleEntry entry)
    {
        bool isProblem = entry.Level >= LogLevel.Warning;
        TextWriter writer = isProblem ? _error : _output;
        bool colored = isProblem ? _colorsOnError : _colorsOnOutput;

        DefaultInterpolatedStringHandler text = new(0, 0, CultureInfo.InvariantCulture, stackalloc char[ConsoleLine.StackLength]);

        try
        {
            ConsoleLine.Append(ref text, in entry, _timestampFormat, colored);
            text.AppendLiteral(Environment.NewLine);

            writer.Write(text.Text);
        }
        finally
        {
            text.Clear();
        }
    }

    private static bool UsesColors(ConsoleColors colors, bool redirected, bool terminal) => colors switch
    {
        ConsoleColors.Always => true,
        ConsoleColors.Never => false,
        ConsoleColors.Auto => !redirected && terminal,
        _ => !redirected && terminal
    };
}
