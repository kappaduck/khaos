// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Hosting.Logging;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Reflection;

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed partial class KhaosEngine : IDisposable
{
    internal const string Category = "KappaDuck.Khaos.Hosting";

    private readonly NativeSession _session;
    private readonly Renderer _renderer;
    private readonly IDisposable? _services;

    private bool _disposed;
    private EventContext _eventContext;
    private bool _started;

    private KhaosEngine(EngineArguments arguments, ILoggerFactory loggers)
    {
        (Game2D game, Window window, WindowOptions windowOptions, GraphicsOptions graphics, EngineOptions engineOptions, IDisposable? services, _) = arguments;

        Game = game;
        Window = window;
        AppMetadata metadata = AppMetadata.FromAppContext();
        ILogger logger = loggers.CreateLogger(Category);

        NativeLog.Attach(loggers);

        try
        {
            _session = NativeSession.Start(metadata, engineOptions, logger);
        }
        catch
        {
            NativeLog.Detach();
            throw;
        }

        try
        {
            _ = NativeHints.Set(NativeHints.MainCallbackRate, graphics.MaxFrameRate.ToString(CultureInfo.InvariantCulture), logger);
            RenderDriver renderDriver = ChooseRenderDriver(engineOptions.RenderDriver, logger);
            window.Open(windowOptions, metadata.Name);

            try
            {
                _renderer = CreateRenderer(window, graphics.VSync, renderDriver, loggers, logger);
                _eventContext = new EventContext(window.ContentScale);
            }
            catch
            {
                window.Close();
                throw;
            }
        }
        catch
        {
            NativeLog.Detach();
            _session.Dispose();
            throw;
        }

        _services = services;
        game.Window = window;

        if (logger.IsEnabled(LogLevel.Information))
        {
            string version = KhaosVersion();
            string video = DriverNames.Display(SDL3.GetCurrentVideoDriver());
            string renderer = DriverNames.Display(_renderer.Name);
            string native = NativeVersion();

            Started(logger, version, video, renderer, native);
        }
    }

    internal Game2D Game { get; }

    internal Window Window { get; }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            if (_started)
                Game.Shutdown();
        }
        finally
        {
            NativeLog.Detach();

            try
            {
                _services?.Dispose();
            }
            finally
            {
                _renderer.Destroy();
                Window.Close();

                _session.Dispose();
            }
        }
    }

    internal void Dispatch(in Event e)
    {
        Game.OnEvent(in e);

        if (e is QuitRequested)
            Game.Exit();
    }

    internal SDL_AppResult Iterate()
    {
        if (Game.ExitRequested)
            return SDL_AppResult.Success;

        Game.Draw(_renderer);
        _renderer.Present();

        return SDL_AppResult.Continue;
    }

    internal void Receive(in SDL_Event native)
    {
        if (ChangesContentScale(native.Type))
            _eventContext = new EventContext(Window.ContentScale);

        if (IsKeyRepeat(in native))
            return;

        if (!EventConverter.TryConvert(in native, in _eventContext, out Event e))
            return;

        Dispatch(in e);

        if (e.TryGetValue(out CustomEvent custom))
            custom.Release();
    }

    internal static KhaosEngine Start(EngineArguments arguments)
    {
        KhaosEngine engine;

        try
        {
            engine = new KhaosEngine(arguments, arguments.Loggers ?? NullLoggerFactory.Instance);
        }
        catch
        {
            arguments.Services?.Dispose();
            throw;
        }

        Game2D game = arguments.Game;
        game.Reset();

        try
        {
            game.Initialize();
        }
        catch
        {
            engine.Dispose();
            throw;
        }

        engine._started = true;

        return engine;
    }

    private static bool ChangesContentScale(SDL_EventType type)
        => type is SDL_EventType.WindowResized or SDL_EventType.WindowPixelSizeChanged or SDL_EventType.WindowDisplayScaleChanged;

    private static RenderDriver ChooseRenderDriver(RenderDriver driver, ILogger logger)
    {
        if (driver == RenderDriver.Auto)
            return RenderDriver.Auto;

        return NativeHints.Set(NativeHints.RenderDriverHint, NativeHints.NameOf(driver), logger) ? driver : RenderDriver.Auto;
    }

    private static Renderer CreateRenderer(Window window, VSync vsync, RenderDriver chosen, ILoggerFactory loggers, ILogger logger)
    {
        if (chosen != RenderDriver.Auto)
        {
            try
            {
                return Renderer.Create(window, vsync, loggers.CreateLogger<Renderer>());
            }
            catch (PlatformException exception)
            {
                NativeHints.DriverMissing(logger, NativeHints.RenderDriverSetting, NativeHints.NameOf(chosen), exception.Message);
            }
        }

        return Renderer.Create(window, vsync, loggers.CreateLogger<Renderer>(), chosen != RenderDriver.Auto);
    }

    private static bool IsKeyRepeat(in SDL_Event native) => native.Type == SDL_EventType.KeyDown && native.Key.Repeat != 0;

    private static string KhaosVersion()
    {
        string? version = typeof(KhaosEngine).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (version is null)
            return "unknown";

        int commit = version.IndexOf('+', StringComparison.Ordinal);
        return commit < 0 ? version : version[..commit];
    }

    private static string NativeVersion()
    {
        int version = SDL3.GetVersion();

        return string.Create(CultureInfo.InvariantCulture, $"{version / 1_000_000}.{version / 1_000 % 1_000}.{version % 1_000}");
    }

    [LoggerMessage(Level = LogLevel.Information, SkipEnabledCheck = true, Message = "Khaos {Version} started on {VideoDriver} with {Renderer} (native {NativeVersion}).")]
    private static partial void Started(ILogger logger, string version, string videoDriver, string renderer, string nativeVersion);
}
