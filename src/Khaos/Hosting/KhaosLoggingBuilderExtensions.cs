// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Hosting;

/// <summary>
/// Sets up how the logs of the game look, from <c>Program.cs</c>.
/// </summary>
public static class KhaosLoggingBuilderExtensions
{
    extension(ILoggingBuilder logging)
    {
        /// <summary>
        /// Sets how each log looks in the terminal and in the browser's console. The <c>logging:console</c> section of
        /// the settings replaces what it sets.
        /// </summary>
        /// <param name="configure">Changes the options of the console.</param>
        /// <returns>The logging builder, to keep setting up the logs.</returns>
        public ILoggingBuilder ConfigureConsole(Action<ConsoleLogOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(logging);
            ArgumentNullException.ThrowIfNull(configure);

            logging.Services.Configure(configure);
            return logging;
        }
    }
}
