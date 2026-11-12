// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Windowing;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Benchmarks.Frame;

internal static partial class Headless
{
    private const string VideoDriver = "SDL_VIDEO_DRIVER";
    private const string Offscreen = "offscreen";
    private const int Override = 2;

    internal static KhaosEngine Start(Game2D game)
    {
        UseOffscreen();

        WindowOptions window = new();
        game.ApplyTo(window);

        return KhaosEngine.Start(new EngineArguments(game, new Window(), window, new GraphicsOptions { VSync = VSync.Disabled }, new EngineOptions(), null, null));
    }

    private static void UseOffscreen()
    {
        if (!SetHintWithPriority(VideoDriver, Offscreen, Override))
            throw new InvalidOperationException("SDL refused the offscreen video driver hint.");
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_SetHintWithPriority", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static safe partial bool SetHintWithPriority(string name, string value, int priority);
}
