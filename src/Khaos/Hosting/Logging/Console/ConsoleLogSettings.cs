// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

internal sealed class ConsoleLogSettings
{
    public string? Colors { get; set; }

    public int? MaxQueueLength { get; set; }

    public string? QueueFullMode { get; set; }

    public string? TimestampFormat { get; set; }

    public bool? UseUtcTimestamp { get; set; }

    internal void ApplyTo(ConsoleLogOptions options)
    {
        options.MaxQueueLength = MaxQueueLength ?? options.MaxQueueLength;
        options.TimestampFormat = TimestampFormat ?? options.TimestampFormat;
        options.UseUtcTimestamp = UseUtcTimestamp ?? options.UseUtcTimestamp;

        if (Colors is not null)
        {
            if (!IsName(Colors, out ConsoleColors colors))
                throw new KhaosException($"The setting {ConsoleLogOptions.Section}:colors must be Auto, Always or Never, but it is '{Colors}'.");

            options.Colors = colors;
        }

        if (QueueFullMode is not null)
        {
            if (!IsName(QueueFullMode, out ConsoleQueueFullMode mode))
                throw new KhaosException($"The setting {ConsoleLogOptions.Section}:queueFullMode must be Wait or Drop, but it is '{QueueFullMode}'.");

            options.QueueFullMode = mode;
        }
    }

    internal static void ThrowIfInvalid(ConsoleLogOptions options)
    {
        if (options.MaxQueueLength < 1)
            throw new KhaosException($"The setting {ConsoleLogOptions.Section}:maxQueueLength must be 1 or more, but it is {options.MaxQueueLength}.");

        if (string.IsNullOrEmpty(options.TimestampFormat))
            return;

        try
        {
            _ = DateTimeOffset.UnixEpoch.ToString(options.TimestampFormat, CultureInfo.InvariantCulture);
        }
        catch (FormatException exception)
        {
            throw new KhaosException($"The setting {ConsoleLogOptions.Section}:timestampFormat is not a time format: '{options.TimestampFormat}'.", exception);
        }
    }

    private static bool IsName<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
        => Enum.TryParse(value, true, out result) && Enum.IsDefined(result) && !char.IsAsciiDigit(value[0]);
}
