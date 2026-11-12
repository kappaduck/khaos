// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

[UnsupportedOSPlatform("browser")]
internal sealed class ConsoleQueue : IDisposable
{
    private const int DrainTimeout = 1500;

    private readonly int _capacity;
    private readonly Queue<ConsoleEntry> _entries = new();
    private readonly ConsoleQueueFullMode _mode;
    private readonly Thread _thread;
    private readonly Action<ConsoleEntry> _write;

    private bool _completed;
    private int _dropped;

    internal ConsoleQueue(Action<ConsoleEntry> write, int capacity, ConsoleQueueFullMode mode)
    {
        _write = write;
        _capacity = capacity;
        _mode = mode;

        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "Khaos console"
        };

        _thread.Start();
    }

    public void Dispose()
    {
        lock (_entries)
        {
            if (_completed)
                return;

            _completed = true;
            Monitor.PulseAll(_entries);
        }

        _ = _thread.Join(DrainTimeout);
    }

    internal void Enqueue(in ConsoleEntry entry)
    {
        if (!TryEnqueue(in entry))
            _write(entry);
    }

    private void Run()
    {
        while (TryDequeue(out ConsoleEntry entry))
            WriteSafely(entry);
    }

    private bool TryDequeue(out ConsoleEntry entry)
    {
        lock (_entries)
        {
            while (_entries.Count == 0 && !_completed)
                Monitor.Wait(_entries);

            if (!_entries.TryDequeue(out entry))
                return false;

            if (_entries.Count == _capacity - 1)
                Monitor.PulseAll(_entries);

            return true;
        }
    }

    private bool TryEnqueue(in ConsoleEntry entry)
    {
        lock (_entries)
        {
            while (_entries.Count >= _capacity && !_completed)
            {
                if (_mode == ConsoleQueueFullMode.Drop)
                {
                    _dropped++;
                    return true;
                }

                Monitor.Wait(_entries);
            }

            if (_completed)
                return false;

            bool writerWaits = _entries.Count == 0;

            if (_dropped > 0)
            {
                _entries.Enqueue(Dropped(entry.Time, _dropped));
                _dropped = 0;
            }

            _entries.Enqueue(entry);

            if (writerWaits)
                Monitor.PulseAll(_entries);

            return true;
        }
    }

    private void WriteSafely(ConsoleEntry entry)
    {
        try
        {
            _write(entry);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"A log could not be written to the console: {exception}");
        }
    }

    private static ConsoleEntry Dropped(DateTimeOffset? time, int count)
    {
        string message = count == 1
            ? "The console could not keep up, so 1 log was dropped."
            : $"The console could not keep up, so {count} logs were dropped.";

        return new ConsoleEntry(time, LogLevel.Warning, message, null);
    }
}
