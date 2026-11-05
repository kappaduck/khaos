// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos.Graphics;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection ConfigureGraphics(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(GraphicsOptions.Section);

            return services.PostConfigure<GraphicsOptions>(options =>
            {
                GraphicsSettings settings = new();

                section.Bind(settings);
                settings.ApplyTo(options);
            });
        }
    }
}
