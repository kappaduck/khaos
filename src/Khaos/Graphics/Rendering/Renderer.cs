// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Graphics.Rendering;

/// <summary>
/// Draws the frame your game shows on the screen.
/// </summary>
/// <remarks>
/// <para>
/// Khaos hands the renderer to your <c>Draw</c> method once per frame, and shows what you drew when <c>Draw</c> returns.
/// </para>
/// <para>
/// Start each frame with <see cref="Clear"/>, then draw from back to front.
/// </para>
/// </remarks>
public sealed partial class Renderer
{
    private SDL_Renderer* _handle;

    private Renderer(SDL_Renderer* handle) => _handle = handle;

    internal SDL_Renderer* Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle is null, this);
            return _handle;
        }
    }

    /// <summary>
    /// Fills the whole screen with one color.
    /// </summary>
    /// <remarks>
    /// Call it first in <c>Draw</c>, even when you draw over every pixel.
    /// </remarks>
    /// <param name="color">The color to fill the screen with.</param>
    /// <exception cref="ObjectDisposedException">The game has exited and the renderer is gone.</exception>
    /// <exception cref="PlatformException">The graphics driver could not clear the screen.</exception>
    public void Clear(Color color)
    {
        // SAFETY: Handle is a renderer that is not destroyed yet.
        SDL3.ThrowIfFailed(unsafe (SDL3.SetRenderDrawColor(Handle, color.R, color.G, color.B, color.A)));
        SDL3.ThrowIfFailed(unsafe (SDL3.RenderClear(Handle)));
    }

    internal void Destroy()
    {
        if (_handle is null)
            return;

        // SAFETY: _handle is not null, so the renderer is not destroyed yet, and it is cleared right after.
        unsafe
        {
            SDL3.DestroyRenderer(_handle);
        }

        _handle = null;
    }

    // SAFETY: Handle is a renderer that is not destroyed yet.
    internal void Present() => SDL3.ThrowIfFailed(unsafe (SDL3.RenderPresent(Handle)));

    internal static Renderer Create(Window window, VSync vsync, ILogger<Renderer> logger)
    {
        // SAFETY: window.Handle is a window that is not destroyed yet, and a null name lets SDL pick the driver.
        Renderer renderer = new(SDL3.ThrowIfNull(unsafe (SDL3.CreateRenderer(window.Handle, null))));

        if (!OperatingSystem.IsBrowser())
            renderer.ApplyVSync(vsync, logger);

        return renderer;
    }

    private static bool CanFallBackToEnabled(VSync vsync) => vsync == VSync.Adaptive || vsync.Interval > 1;

    private void ApplyVSync(VSync vsync, ILogger logger)
    {
        if (TrySetVSync(vsync))
            return;

        if (CanFallBackToEnabled(vsync) && TrySetVSync(VSync.Enabled))
        {
            VSyncFellBackToEveryRefresh(logger, vsync);
            return;
        }

        VSyncRefused(logger, vsync);
    }

    // SAFETY: Handle is a renderer that is not destroyed yet.
    private bool TrySetVSync(VSync vsync) => unsafe (SDL3.SetRenderVSync(Handle, vsync.Interval));

    [LoggerMessage(Level = LogLevel.Information, Message = "The graphics driver does not support VSync {VSync}: the game waits for every refresh of the screen instead.")]
    private static partial void VSyncFellBackToEveryRefresh(ILogger logger, VSync vsync);

    [LoggerMessage(Level = LogLevel.Information, Message = "The graphics driver does not support VSync {VSync}: the game shows each frame without waiting for the screen.")]
    private static partial void VSyncRefused(ILogger logger, VSync vsync);
}
