// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting.Logging;
using KappaDuck.Khaos.Hosting.Logging.Console;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Hosting;

/// <summary>
/// Collects the services and the settings of a game, then builds the <see cref="KhaosApplication"/> that runs it.
/// </summary>
public sealed class KhaosApplicationBuilder
{
    private bool _built;

    internal KhaosApplicationBuilder(string[] args, string contentRoot)
    {
        GameEnvironment environment = GameEnvironment.Resolve(args, contentRoot);
        Environment = environment;

        Configuration = new ConfigurationManager();
        Configuration.SetBasePath(contentRoot);
        Configuration.AddJsonFile("appsettings.json", true, false);
        Configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, false);
        Configuration.AddEnvironmentVariables(GameEnvironment.KhaosPrefix);
        Configuration.AddCommandLine(args);

        ServiceCollection services = [];

        services.AddSingleton<IHostEnvironment>(environment);
        services.BindWindowSettings(Configuration);
        services.BindGraphicsSettings(Configuration);
        services.BindLoopSettings(Configuration);
        services.BindEngineSettings(Configuration);
        services.BindConsoleSettings(Configuration);
        services.AddLogging();

        Services = services;

        Logging = new LoggingBuilder(services);
        Logging.SetMinimumLevel(environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information);
        Logging.AddFilter(NativeLog.Category, LogLevel.Warning);
        Logging.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, KhaosConsoleLoggerProvider>(KhaosConsoleLoggerProvider.Create));
    }

    /// <summary>
    /// Gets the settings of the game, from the strongest to the weakest: the command line (the page address in the
    /// browser), the environment variables that start with <c>KHAOS_</c>, <c>appsettings.{environment}.json</c> and
    /// <c>appsettings.json</c>.
    /// </summary>
    public ConfigurationManager Configuration { get; }

    /// <summary>
    /// Gets the environment the game runs in, such as <c>Development</c> or <c>Production</c>.
    /// </summary>
    public IHostEnvironment Environment { get; }

    /// <summary>
    /// Gets the logging of the game: where messages go and which ones are kept. The <c>logging</c> section of the
    /// settings wins over what you set here.
    /// </summary>
    public ILoggingBuilder Logging { get; }

    /// <summary>
    /// Gets the services of the game. Register yours here, and the game's constructor can ask for them.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Builds the application that runs <typeparamref name="TGame"/>. Khaos creates the game, so its constructor can
    /// ask for any registered service.
    /// </summary>
    /// <typeparam name="TGame">The game to run.</typeparam>
    /// <returns>The application, ready to run.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Build{TGame}"/> was already called.</exception>
    /// <exception cref="AggregateException">
    /// A service cannot be created, because something it needs is not registered or the game reads the window options
    /// in its constructor.
    /// </exception>
    public KhaosApplication Build<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGame>()
        where TGame : Game2D
    {
        if (_built)
            throw new InvalidOperationException("The application is already built. Call Build once, then Run on what it returns.");

        _built = true;

        Logging.AddConfiguration(Configuration.GetSection("logging"));

        Window window = new();

        Services.AddSingleton<IConfiguration>(Configuration);
        Services.AddSingleton(window);
        Services.TryAddSingleton(TimeProvider.System);
        Services.AddSingleton<TGame>();
        Services.Insert(0, ServiceDescriptor.Singleton<IConfigureOptions<WindowOptions>, GameWindowDefaults<TGame>>());

        ServiceProvider provider = Services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        return new KhaosApplication(provider, window, static services => services.GetRequiredService<TGame>());
    }

    private sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        public IServiceCollection Services { get; } = services;
    }
}
