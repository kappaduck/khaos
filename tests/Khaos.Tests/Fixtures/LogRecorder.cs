// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace KappaDuck.Khaos.Tests.Fixtures;

internal sealed record RecordedLog(string Category, LogLevel Level, string Message);

internal sealed class LogRecorder : ILoggerProvider
{
    private readonly ConcurrentQueue<RecordedLog> _entries = new();

    internal IReadOnlyCollection<RecordedLog> Entries => _entries;

    public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, _entries);

    public void Dispose()
    {
    }

    internal ILoggerFactory Factory(LogLevel minimum) => LoggerFactory.Create(logging => logging.SetMinimumLevel(minimum).AddProvider(this));

    private sealed class RecordingLogger(string category, ConcurrentQueue<RecordedLog> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => entries.Enqueue(new RecordedLog(category, logLevel, formatter(state, exception)));
    }
}
