// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

[ProviderAlias("Console")]
internal sealed class KhaosConsoleLoggerProvider(ConsoleLogOptions options, TimeProvider time, IConsole console) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new KhaosConsoleLogger(this);

    public void Dispose() => console.Dispose();

    internal static KhaosConsoleLoggerProvider Create(IServiceProvider services)
    {
        ConsoleLogOptions options = services.GetRequiredService<IOptions<ConsoleLogOptions>>().Value;
        IConsole console = OperatingSystem.IsBrowser() ? new BrowserConsole(options.TimestampFormat) : new DesktopConsole(options);

        return new KhaosConsoleLoggerProvider(options, TimeProvider.System, console);
    }

    private DateTimeOffset? Now()
    {
        if (string.IsNullOrEmpty(options.TimestampFormat))
            return null;

        return options.UseUtcTimestamp ? time.GetUtcNow() : time.GetLocalNow();
    }

    private void Write(LogLevel level, string message, Exception? exception)
    {
        ConsoleEntry entry = new(Now(), level, message, exception);
        console.Write(in entry);
    }

    private sealed class KhaosConsoleLogger(KhaosConsoleLoggerProvider provider) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            provider.Write(logLevel, formatter(state, exception), exception);
        }
    }
}
