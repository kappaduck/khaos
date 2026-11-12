// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using BenchmarkDotNet.Attributes;
using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Logging;
using System.Text;

namespace KappaDuck.Khaos.Benchmarks.Micro.Hosting.Logging.Console;

public class DesktopConsoleBenchmarks
{
    private const int Logs = 100_000;
    private const string TimestampFormat = "HH:mm:ss.fff";

    private static readonly ConsoleEntry Scored = new(DateTimeOffset.UnixEpoch, LogLevel.Debug, "Score is now 10.", null);

    private DesktopConsole _console = null!;
    private long _enqueued;
    private CountingWriter _sink = null!;

    [GlobalSetup]
    public void OpenTheNullDevice()
    {
        FileStream device = new(OperatingSystem.IsWindows() ? "NUL" : "/dev/null", FileMode.Open, FileAccess.Write);
        _sink = new CountingWriter(new StreamWriter(device) { AutoFlush = true });

        ConsoleLogOptions options = new() { TimestampFormat = TimestampFormat, MaxQueueLength = Logs * 2 };
        _console = new DesktopConsole(options, _sink, _sink, false, false);
    }

    [GlobalCleanup]
    public void CloseTheNullDevice()
    {
        _console.Dispose();
        _sink.Dispose();
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = Logs)]
    public long WriteDirectly()
    {
        for (int i = 0; i < Logs; i++)
            _sink.WriteLine(ConsoleLine.Format(in Scored, TimestampFormat, false));

        return _sink.Lines;
    }

    [Benchmark(OperationsPerInvoke = Logs)]
    public long Enqueue()
    {
        for (int i = 0; i < Logs; i++)
            _console.Write(in Scored);

        _enqueued += Logs;
        return _enqueued;
    }

    [IterationCleanup(Target = nameof(Enqueue))]
    public void WaitForTheConsole() => SpinWait.SpinUntil(() => _sink.Lines >= _enqueued);

    private sealed class CountingWriter(TextWriter inner) : TextWriter
    {
        private long _lines;

        public override Encoding Encoding => inner.Encoding;

        internal long Lines => Interlocked.Read(ref _lines);

        public override void WriteLine(string? value)
        {
            inner.WriteLine(value);
            _ = Interlocked.Increment(ref _lines);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inner.Dispose();

            base.Dispose(disposing);
        }
    }
}
