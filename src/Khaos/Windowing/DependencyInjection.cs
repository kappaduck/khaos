// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos.Windowing;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection BindWindowSettings(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(WindowOptions.Section);

            return services.PostConfigure<WindowOptions>(options =>
            {
                WindowSettings settings = new();

                section.Bind(settings);
                settings.ApplyTo(options);

                ThrowIfNotPositive(options.Width, "width");
                ThrowIfNotPositive(options.Height, "height");
            });
        }
    }

    private static void ThrowIfNotPositive(int value, string key)
    {
        if (value <= 0)
            throw new KhaosException($"The setting {WindowOptions.Section}:{key} must be greater than 0, but it is {value}.");
    }
}
