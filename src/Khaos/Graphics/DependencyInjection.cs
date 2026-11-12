// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos.Graphics;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection BindGraphicsSettings(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(GraphicsOptions.Section);

            return services.PostConfigure<GraphicsOptions>(options =>
            {
                GraphicsSettings settings = new();

                section.Bind(settings);
                settings.ApplyTo(options);

                if (options.MaxFrameRate < 0)
                    throw new KhaosException($"The setting {GraphicsOptions.Section}:maxFrameRate must be 0 or more, but it is {options.MaxFrameRate}.");
            });
        }
    }
}
