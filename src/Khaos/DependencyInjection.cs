// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection BindLoopSettings(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(LoopOptions.Section);

            return services.PostConfigure<LoopOptions>(options =>
            {
                LoopSettings settings = new();

                section.Bind(settings);
                settings.ApplyTo(options);

                LoopSettings.ThrowIfInvalid(options);
            });
        }
    }
}
