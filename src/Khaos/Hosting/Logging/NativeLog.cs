// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KappaDuck.Khaos.Hosting.Logging;

internal static partial class NativeLog
{
    internal const string Category = "KappaDuck.Khaos.Native";

    private static readonly string[] _categories =
    [
        $"{Category}.Application",
        $"{Category}.Error",
        $"{Category}.Assert",
        $"{Category}.System",
        $"{Category}.Audio",
        $"{Category}.Video",
        $"{Category}.Render",
        $"{Category}.Input",
        $"{Category}.Test",
        $"{Category}.Gpu"
    ];

    private static ILogger[] _loggers = [];
    private static ILogger _other = NullLogger.Instance;

    internal static void Attach(ILoggerFactory loggers)
    {
        ILogger[] byCategory = new ILogger[_categories.Length];

        for (int category = 0; category < byCategory.Length; category++)
        {
            byCategory[category] = loggers.CreateLogger(_categories[category]);
            SDL3.SetLogPriority((SDL_LogCategory)category, LowestPriorityOf(byCategory[category]));
        }

        _loggers = byCategory;
        _other = loggers.CreateLogger(Category);

        // SAFETY: Write matches SDL_LogOutputFunction, and a null userdata is never read.
        unsafe
        {
            SDL3.SetLogOutputFunction(&Write, null);
        }
    }

    internal static void Detach()
    {
        // SAFETY: SDL's own output function takes a null userdata, as SDL sets it before any custom one.
        unsafe
        {
            SDL3.SetLogOutputFunction(SDL3.GetDefaultLogOutputFunction(), null);
        }

        _loggers = [];
        _other = NullLogger.Instance;
    }

    internal static LogLevel LevelOf(SDL_LogPriority priority) => priority switch
    {
        SDL_LogPriority.Trace or SDL_LogPriority.Verbose => LogLevel.Trace,
        SDL_LogPriority.Debug => LogLevel.Debug,
        SDL_LogPriority.Info => LogLevel.Information,
        SDL_LogPriority.Warn => LogLevel.Warning,
        SDL_LogPriority.Error => LogLevel.Error,
        SDL_LogPriority.Critical => LogLevel.Critical,
        SDL_LogPriority.Invalid or SDL_LogPriority.Count => LogLevel.Information,
        _ => LogLevel.Information
    };

    internal static SDL_LogPriority LowestPriorityOf(ILogger logger)
    {
        if (logger.IsEnabled(LogLevel.Trace))
            return SDL_LogPriority.Trace;

        if (logger.IsEnabled(LogLevel.Debug))
            return SDL_LogPriority.Debug;

        if (logger.IsEnabled(LogLevel.Information))
            return SDL_LogPriority.Info;

        if (logger.IsEnabled(LogLevel.Warning))
            return SDL_LogPriority.Warn;

        if (logger.IsEnabled(LogLevel.Error))
            return SDL_LogPriority.Error;

        return logger.IsEnabled(LogLevel.Critical) ? SDL_LogPriority.Critical : SDL_LogPriority.Count;
    }

    private static ILogger LoggerOf(SDL_LogCategory category)
    {
        ILogger[] loggers = _loggers;
        int index = (int)category;

        return index >= 0 && index < loggers.Length ? loggers[index] : _other;
    }

    [UnmanagedCallersOnly]
    private static void Write(void* userdata, SDL_LogCategory category, SDL_LogPriority priority, byte* message)
    {
        try
        {
            ILogger logger = LoggerOf(category);
            LogLevel level = LevelOf(priority);

            if (!logger.IsEnabled(level))
                return;

            // SAFETY: SDL passes a null-terminated message it owns, valid until the callback returns.
            string text = unsafe (Utf8StringMarshaller.ConvertToManaged(message)) ?? string.Empty;
            Message(logger, level, text);
        }
        catch (Exception exception)
        {
            System.Console.Error.WriteLine($"A native log could not be written: {exception}");
        }
    }

    [LoggerMessage(Message = "{Message}")]
    private static partial void Message(ILogger logger, LogLevel level, string message);
}
