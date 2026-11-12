// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos.Hosting;

internal static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection BindConsoleSettings(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(ConsoleLogOptions.Section);

            return services.PostConfigure<ConsoleLogOptions>(options =>
            {
                ConsoleLogSettings settings = new();

                section.Bind(settings);
                settings.ApplyTo(options);

                ConsoleLogSettings.ThrowIfInvalid(options);
            });
        }

        internal IServiceCollection BindEngineSettings(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(EngineOptions.Section);
            IConfigurationSection native = section.GetSection(EngineOptions.NativeSection);

            return services.PostConfigure<EngineOptions>(options =>
            {
                EngineSettings settings = new();

                section.Bind(settings);
                settings.ApplyTo(options);

                foreach (IConfigurationSection setting in native.GetChildren())
                    options.Native[setting.Key] = setting.Value ?? throw new KhaosException($"The setting {setting.Path} needs a value.");

                NativeHints.ThrowIfRefused(options.Native);
            });
        }
    }
}
