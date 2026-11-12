// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KappaDuck.Khaos.Hosting;

/// <summary>
/// A game with its services and settings, ready to run.
/// </summary>
public sealed class KhaosApplication
{
    private readonly ServiceProvider _provider;
    private readonly Func<IServiceProvider, Game2D> _resolveGame;
    private readonly Window _window;

    private bool _ran;

    internal KhaosApplication(ServiceProvider provider, Window window, Func<IServiceProvider, Game2D> resolveGame)
    {
        _provider = provider;
        _window = window;
        _resolveGame = resolveGame;
    }

    /// <summary>
    /// Creates the game, opens its window and runs it until it exits.
    /// </summary>
    /// <remarks>
    /// In the browser, it returns as soon as the first frame is scheduled, and the game keeps running.
    /// </remarks>
    /// <returns>The exit code of the game: 0 when it exits normally.</returns>
    /// <exception cref="InvalidOperationException">The application has already run, or another game is running.</exception>
    /// <exception cref="KhaosException">A setting of the window, the graphics, the loop or the engine is not valid.</exception>
    public int Run()
    {
        if (_ran)
            throw new InvalidOperationException("The application has already run. An application runs its game once.");

        _ran = true;

        EngineArguments arguments;

        try
        {
            GameRunner.ThrowIfAGameRuns();

            arguments = new EngineArguments(
                _resolveGame(_provider),
                _window,
                _provider.GetRequiredService<IOptions<WindowOptions>>().Value,
                _provider.GetRequiredService<IOptions<GraphicsOptions>>().Value,
                _provider.GetRequiredService<IOptions<LoopOptions>>().Value,
                _provider.GetRequiredService<IOptions<EngineOptions>>().Value,
                _provider.GetRequiredService<TimeProvider>(),
                _provider,
                _provider.GetRequiredService<ILoggerFactory>());
        }
        catch
        {
            _provider.Dispose();
            throw;
        }

        return GameRunner.Run(arguments);
    }

    /// <summary>
    /// Starts building the application of a game.
    /// </summary>
    /// <param name="args">The command line of the game.</param>
    /// <returns>The builder, to add services and settings to.</returns>
    /// <exception cref="KhaosException">The name of the environment is empty.</exception>
    public static KhaosApplicationBuilder CreateBuilder(string[]? args = null) => new(args ?? [], AppContext.BaseDirectory);

    internal IServiceProvider Services => _provider;
}
