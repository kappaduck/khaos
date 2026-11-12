// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Hosting;

/// <summary>
/// Sets up the game from <c>Program.cs</c>: the engine, the graphics, the logging, the loop and the window.
/// </summary>
public static class KhaosApplicationBuilderExtensions
{
    extension(KhaosApplicationBuilder builder)
    {
        /// <summary>
        /// Sets how the engine starts. The <c>engine</c> section of the settings replaces what it sets.
        /// </summary>
        /// <param name="configure">Changes the options of the engine.</param>
        /// <returns>The builder, to keep setting up the game.</returns>
        public KhaosApplicationBuilder ConfigureEngine(Action<EngineOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            builder.Services.Configure(configure);
            return builder;
        }

        /// <summary>
        /// Sets how the game draws on the screen. The <c>graphics</c> section of the settings replaces what it sets.
        /// </summary>
        /// <param name="configure">Changes the options of the graphics.</param>
        /// <returns>The builder, to keep setting up the game.</returns>
        public KhaosApplicationBuilder ConfigureGraphics(Action<GraphicsOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            builder.Services.Configure(configure);
            return builder;
        }

        /// <summary>
        /// Sets up where log messages go and which ones are kept. The <c>logging</c> section of the settings wins over
        /// what it sets.
        /// </summary>
        /// <param name="configure">Changes the logging of the game.</param>
        /// <returns>The builder, to keep setting up the game.</returns>
        public KhaosApplicationBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            configure(builder.Logging);
            return builder;
        }

        /// <summary>
        /// Sets how the game loop keeps time, such as the rate of the fixed steps. The <c>loop</c> section of the
        /// settings replaces what it sets.
        /// </summary>
        /// <param name="configure">Changes the options of the loop.</param>
        /// <returns>The builder, to keep setting up the game.</returns>
        public KhaosApplicationBuilder ConfigureLoop(Action<LoopOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            builder.Services.Configure(configure);
            return builder;
        }

        /// <summary>
        /// Sets how the game window opens. The <c>window</c> section of the settings replaces what it sets.
        /// </summary>
        /// <param name="configure">Changes the options of the window.</param>
        /// <returns>The builder, to keep setting up the game.</returns>
        public KhaosApplicationBuilder ConfigureWindow(Action<WindowOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);

            builder.Services.Configure(configure);
            return builder;
        }
    }
}
