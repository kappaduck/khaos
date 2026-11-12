// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos;

internal sealed class LoopSettings
{
    private const double MostFrameTime = 1;

    public int? FixedRate { get; set; }

    public double? MaxFrameTime { get; set; }

    public bool? PauseWhenMinimized { get; set; }

    internal void ApplyTo(LoopOptions options)
    {
        options.FixedRate = FixedRate ?? options.FixedRate;
        options.MaxFrameTime = MaxFrameTime ?? options.MaxFrameTime;
        options.PauseWhenMinimized = PauseWhenMinimized ?? options.PauseWhenMinimized;
    }

    internal static void ThrowIfInvalid(LoopOptions options)
    {
        if (options.FixedRate <= 0)
            throw new KhaosException($"The setting {LoopOptions.Section}:fixedRate must be 1 or more, but it is {options.FixedRate}.");

        double step = 1.0 / options.FixedRate;

        if (options.MaxFrameTime < step || options.MaxFrameTime > MostFrameTime)
        {
            string value = options.MaxFrameTime.ToString(CultureInfo.InvariantCulture);
            throw new KhaosException($"The setting {LoopOptions.Section}:maxFrameTime must be between one fixed step and {MostFrameTime} second, but it is {value}.");
        }
    }
}
