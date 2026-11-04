// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Buffers;
using System.Text;

namespace KappaDuck.Khaos.Windowing;

/// <summary>
/// The window your game runs in, with its title, its size, and whether it is fullscreen.
/// </summary>
/// <remarks>
/// <para>
/// Khaos opens the window as <see cref="WindowOptions"/> say and closes it when the game exits. Change it here while
/// the game runs.
/// </para>
/// <para>
/// Sizes and positions are in logical units, so a 1280 × 720 window looks the same size on every screen, whatever its
/// scale setting.
/// </para>
/// <para>
/// The operating system applies some changes, such as a new size or fullscreen, a moment later, and may adjust them.
/// </para>
/// </remarks>
public sealed class Window
{
    private const uint CenteredMask = 0x2FFF0000u;
    private const int MaxTitleBytes = 256;

    private SDL_Window* _handle;

    private Window(SDL_Window* handle) => _handle = handle;

    /// <summary>
    /// Gets or sets a value indicating whether the window has no title bar and no borders.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public bool Borderless
    {
        get => Has(SDL_WindowStates.Borderless);

        // SAFETY: Handle is a window that is not destroyed yet.
        set => SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowBordered(Handle, !value)));
    }

    /// <summary>
    /// Gets a value indicating whether the game can move the window with <see cref="Position"/>, <see cref="MoveTo"/>
    /// and <see cref="Display"/>.
    /// </summary>
    /// <remarks>
    /// <see langword="false"/> on Linux with Wayland and in the browser, where the system places the window itself.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    public bool CanMove
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle is null, this);
            return SDL3.GetCurrentVideoDriver() is "windows" or "x11";
        }
    }

    /// <summary>
    /// Gets or sets the screen the window is on.
    /// </summary>
    /// <remarks>
    /// Setting a screen centers the window on it. To move a maximized window, use <see cref="Restore"/> first. Where
    /// <see cref="CanMove"/> is <see langword="false"/>, it does nothing.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The screen could not be read, or the operating system refused the change.</exception>
    public Display Display
    {
        // SAFETY: Handle is a window that is not destroyed yet.
        get => new(SDL3.ThrowIfZero(unsafe (SDL3.GetDisplayForWindow(Handle))));
        set => SDL3.ThrowIfFailed(TryPlaceOn(value, null, null) || !CanMove);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the window can take the keyboard focus.
    /// </summary>
    /// <remarks>
    /// <see langword="true"/> by default. A window that cannot take the focus receives no keys, which suits an overlay.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public bool Focusable
    {
        get => !Has(SDL_WindowStates.NotFocusable);

        // SAFETY: Handle is a window that is not destroyed yet.
        set => SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowFocusable(Handle, value)));
    }

    /// <summary>
    /// Gets a value indicating whether the window has the keyboard focus, which means the player is using it.
    /// </summary>
    /// <remarks>
    /// Check it to pause the game when the player switches to another window.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    public bool Focused => Has(SDL_WindowStates.InputFocus);

    /// <summary>
    /// Gets or sets a value indicating whether the window covers the whole screen.
    /// </summary>
    /// <remarks>
    /// The window covers the screen it is on, and the screen keeps its resolution.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public bool Fullscreen
    {
        get => Has(SDL_WindowStates.Fullscreen);

        // SAFETY: Handle is a window that is not destroyed yet.
        set => SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowFullscreen(Handle, value)));
    }

    /// <summary>
    /// Gets a value indicating whether the window is hidden, with no trace on the desktop or in the taskbar.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    public bool Hidden => Has(SDL_WindowStates.Hidden);

    /// <summary>
    /// Gets a value indicating whether the window is maximized.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    public bool Maximized => Has(SDL_WindowStates.Maximized);

    /// <summary>
    /// Gets or sets the largest size the player can give the window, in logical units.
    /// </summary>
    /// <remarks>
    /// A width or a height of 0, the default, means no limit on that side.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The width or the height is negative.</exception>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public SizeI MaximumSize
    {
        get
        {
            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.GetWindowMaximumSize(Handle, out int width, out int height)));

            return WindowScale.ToLogical(new SizeI(width, height), GetNativeSize().ContentScale);
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value.Width);
            ArgumentOutOfRangeException.ThrowIfNegative(value.Height);

            SizeI native = WindowScale.ToNative(value, GetNativeSize().ContentScale);

            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowMaximumSize(Handle, native.Width, native.Height)));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the window is minimized to the taskbar.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    public bool Minimized => Has(SDL_WindowStates.Minimized);

    /// <summary>
    /// Gets or sets the smallest size the player can give the window, in logical units.
    /// </summary>
    /// <remarks>
    /// A width or a height of 0, the default, means no limit on that side.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The width or the height is negative.</exception>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public SizeI MinimumSize
    {
        get
        {
            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.GetWindowMinimumSize(Handle, out int width, out int height)));

            return WindowScale.ToLogical(new SizeI(width, height), GetNativeSize().ContentScale);
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value.Width);
            ArgumentOutOfRangeException.ThrowIfNegative(value.Height);

            SizeI native = WindowScale.ToNative(value, GetNativeSize().ContentScale);

            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowMinimumSize(Handle, native.Width, native.Height)));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the window is completely covered by other windows, or not visible at all.
    /// </summary>
    /// <remarks>
    /// Nobody can see the game while it is <see langword="true"/>, which makes it a good moment to pause.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    public bool Occluded => Has(SDL_WindowStates.Occluded);

    /// <summary>
    /// Gets or sets how opaque the whole window is, from 0 (invisible) to 1 (fully opaque, the default).
    /// </summary>
    /// <remarks>
    /// Where the operating system cannot make a window see-through, it stays opaque.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value is below 0 or above 1.</exception>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public float Opacity
    {
        // SAFETY: Handle is a window that is not destroyed yet.
        get => SDL3.ThrowIfNegative(unsafe (SDL3.GetWindowOpacity(Handle)));
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, 1f);

            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowOpacity(Handle, value)));
        }
    }

    /// <summary>
    /// Gets how many real pixels the screen has for each logical unit of the window, such as 1 at 100 % or 2 at 200 %.
    /// </summary>
    /// <remarks>
    /// It changes when the player moves the window to another screen or changes the scale setting of their screen.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The density could not be read.</exception>
    // SAFETY: Handle is a window that is not destroyed yet.
    public float PixelDensity => SDL3.ThrowIfZero(unsafe (SDL3.GetWindowDisplayScale(Handle)));

    /// <summary>
    /// Gets or sets where the window is on its screen, in logical units from the top-left corner of
    /// <see cref="Display"/>.
    /// </summary>
    /// <remarks>
    /// To move a maximized window, use <see cref="Restore"/> first. Where <see cref="CanMove"/> is
    /// <see langword="false"/>, setting it does nothing.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The position could not be read, or the operating system refused the change.</exception>
    public PointI Position
    {
        get
        {
            Display display = Display;

            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.GetWindowPosition(Handle, out int x, out int y)));

            RectI bounds = display.NativeBounds();
            return WindowScale.ToLogical(new PointI(x - bounds.X, y - bounds.Y), display.ContentScale());
        }
        set => MoveTo(Display, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the player can resize the window by dragging its edges.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public bool Resizable
    {
        get => Has(SDL_WindowStates.Resizable);

        // SAFETY: Handle is a window that is not destroyed yet.
        set => SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowResizable(Handle, value)));
    }

    /// <summary>
    /// Gets or sets the size of the inside of the window, without its borders and title bar, in logical units.
    /// </summary>
    /// <remarks>
    /// The operating system can keep the window smaller than asked, for example to fit it on the screen.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The width or the height is 0 or negative.</exception>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public SizeI Size
    {
        get
        {
            (SizeI native, float contentScale) = GetNativeSize();
            return WindowScale.ToLogical(native, contentScale);
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value.Width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value.Height);

            SizeI native = WindowScale.ToNative(value, GetNativeSize().ContentScale);

            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowSize(Handle, native.Width, native.Height)));
        }
    }

    /// <summary>
    /// Gets the size of the inside of the window in real pixels on the screen.
    /// </summary>
    /// <remarks>
    /// Equal to <see cref="Size"/> on a screen at 100 %; twice as large on a screen at 200 %.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The size could not be read.</exception>
    public SizeI SizeInPixels
    {
        get
        {
            // SAFETY: Handle is a window that is not destroyed yet.
            SDL3.ThrowIfFailed(unsafe (SDL3.GetWindowSizeInPixels(Handle, out int width, out int height)));
            return new SizeI(width, height);
        }
    }

    /// <summary>
    /// Gets or sets the text in the title bar of the window.
    /// </summary>
    /// <remarks>
    /// For a title that changes all the time, such as a frame counter, use
    /// <see cref="SetTitle(ref DefaultInterpolatedStringHandler)"/>.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public string Title
    {
        // SAFETY: Handle is a window that is not destroyed yet, and SDL keeps the title it returns alive until the next title change.
        get => unsafe (SDL3.GetWindowTitle(Handle)) ?? string.Empty;
        set => SetTitle(value);
    }

    internal SDL_Window* Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle is null, this);
            return _handle;
        }
    }

    /// <summary>
    /// Hides the window from the desktop and the taskbar. The game keeps running.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    // SAFETY: Handle is a window that is not destroyed yet.
    public void Hide() => SDL3.ThrowIfFailed(unsafe (SDL3.HideWindow(Handle)));

    /// <summary>
    /// Makes the window as large as the screen allows, keeping its title bar and borders.
    /// </summary>
    /// <remarks>
    /// Only a window the player can resize can be maximized; see <see cref="Resizable"/>.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    // SAFETY: Handle is a window that is not destroyed yet.
    public void Maximize() => SDL3.ThrowIfFailed(unsafe (SDL3.MaximizeWindow(Handle)));

    /// <summary>
    /// Minimizes the window to the taskbar.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    // SAFETY: Handle is a window that is not destroyed yet.
    public void Minimize() => SDL3.ThrowIfFailed(unsafe (SDL3.MinimizeWindow(Handle)));

    /// <summary>
    /// Moves the window to a screen, at a position on that screen in logical units.
    /// </summary>
    /// <remarks>
    /// The same as setting <see cref="Display"/> then <see cref="Position"/>, in one move. To move a maximized window,
    /// use <see cref="Restore"/> first. Where <see cref="CanMove"/> is <see langword="false"/>, it does nothing.
    /// </remarks>
    /// <param name="display">The screen to move the window to.</param>
    /// <param name="position">The distance from the top-left corner of that screen to the top-left corner of the window.</param>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The screen could not be read, or the operating system refused the change.</exception>
    public void MoveTo(Display display, PointI position)
        => SDL3.ThrowIfFailed(TryPlaceOn(display, position.X, position.Y) || !CanMove);

    /// <summary>
    /// Brings a maximized or minimized window back to its size and place before.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    // SAFETY: Handle is a window that is not destroyed yet.
    public void Restore() => SDL3.ThrowIfFailed(unsafe (SDL3.RestoreWindow(Handle)));

    /// <summary>
    /// Changes the text in the title bar of the window, from an interpolated string.
    /// </summary>
    /// <remarks>
    /// Use it for a title that changes all the time, such as a frame counter. For a title that rarely changes, setting
    /// <see cref="Title"/> is just as good.
    /// </remarks>
    /// <param name="title">The new title, as an interpolated string.</param>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public void SetTitle(ref DefaultInterpolatedStringHandler title)
    {
        try
        {
            SetTitle(title.Text);
        }
        finally
        {
            title.Clear();
        }
    }

    /// <summary>
    /// Changes the text in the title bar of the window, from characters you already have.
    /// </summary>
    /// <remarks>
    /// For text you already hold, such as the name a player is typing.
    /// </remarks>
    /// <param name="title">The new title.</param>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    public void SetTitle(ReadOnlySpan<char> title)
    {
        int maxBytes = Encoding.UTF8.GetMaxByteCount(title.Length) + 1;
        byte[]? rented = maxBytes > MaxTitleBytes ? ArrayPool<byte>.Shared.Rent(maxBytes) : null;

        Span<byte> bytes = rented ?? stackalloc byte[MaxTitleBytes];

        try
        {
            int written = Encoding.UTF8.GetBytes(title, bytes);
            bytes[written] = 0;

            SDL3.ThrowIfFailed(unsafe (SDL3.SetWindowTitle(Handle, in bytes[0])));
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Shows a window hidden with <see cref="Hide"/>.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The game has exited and the window is closed.</exception>
    /// <exception cref="PlatformException">The operating system refused the change.</exception>
    // SAFETY: Handle is a window that is not destroyed yet.
    public void Show() => SDL3.ThrowIfFailed(unsafe (SDL3.ShowWindow(Handle)));

    internal void Close()
    {
        if (_handle is null)
            return;

        // SAFETY: _handle is not null, so the window is not destroyed yet, and it is cleared right after.
        unsafe
        {
            SDL3.DestroyWindow(_handle);
        }

        _handle = null;
    }

    // SAFETY: Handle is a window that is not destroyed yet.
    internal bool Sync() => unsafe (SDL3.SyncWindow(Handle));

    internal static Window Open(WindowOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Height);

        if (SDL3.WasInit(SDL_Subsystems.Video) == SDL_Subsystems.None)
            throw new InvalidOperationException("The engine is not started. Start it before opening a window.");

        Display display = options.Display is { Connected: true } chosen ? chosen : Display.Primary;
        Window window = new(Create(options, display));

        try
        {
            window.ScaleToLogical(new SizeI(options.Width, options.Height));
            window.TryPlaceOn(display, options.Position?.X, options.Position?.Y);

            window.Show();
        }
        catch
        {
            window.Close();
            throw;
        }

        return window;
    }

    private (SizeI Native, float ContentScale) GetNativeSize()
    {
        SDL3.ThrowIfFailed(unsafe (SDL3.GetWindowSize(Handle, out int width, out int height)));

        float contentScale = WindowScale.ContentScale(PixelDensity, width, SizeInPixels.Width);
        return (new SizeI(width, height), contentScale);
    }

    // SAFETY: Handle is a window that is not destroyed yet.
    private bool Has(SDL_WindowStates state) => (unsafe (SDL3.GetWindowFlags(Handle)) & state) == state;

    private void ScaleToLogical(SizeI logical)
    {
        if (WindowScale.ToNative(logical, GetNativeSize().ContentScale) != logical)
            Size = logical;
    }

    private bool TryMoveOn(Display display, int? x, int? y)
    {
        SDL_Window* window = Handle;

        int centered = CenteredOn(display.Id);

        RectI bounds = display.NativeBounds();
        PointI offset = WindowScale.ToNative(new PointI(x ?? 0, y ?? 0), display.ContentScale());

        int nativeX = x is null ? centered : bounds.X + offset.X;
        int nativeY = y is null ? centered : bounds.Y + offset.Y;

        // SAFETY: window comes from Handle, so it is not destroyed yet.
        return unsafe (SDL3.SetWindowPosition(window, nativeX, nativeY));
    }

    private bool TryPlaceOn(Display display, int? x, int? y)
    {
        Display target = display.Connected ? display : Display.Primary;
        SizeI logical = Size;

        if (!TryMoveOn(target, x, y))
            return false;

        if (Size == logical)
            return true;

        Size = logical;
        return TryMoveOn(target, x, y);
    }

    private static int CenteredOn(uint display) => (int)(CenteredMask | display);

    private static SDL_Window* Create(WindowOptions options, Display display)
    {
        uint properties = SDL3.ThrowIfZero(SDL3.CreateProperties());
        int centered = CenteredOn(display.Id);

        try
        {
            SDL3.ThrowIfFailed(SDL3.SetStringProperty(properties, "SDL.window.create.title", options.Title));
            SDL3.ThrowIfFailed(SDL3.SetNumberProperty(properties, "SDL.window.create.width", options.Width));
            SDL3.ThrowIfFailed(SDL3.SetNumberProperty(properties, "SDL.window.create.height", options.Height));
            SDL3.ThrowIfFailed(SDL3.SetNumberProperty(properties, "SDL.window.create.flags", ToState(options)));
            SDL3.ThrowIfFailed(SDL3.SetNumberProperty(properties, "SDL.window.create.x", centered));
            SDL3.ThrowIfFailed(SDL3.SetNumberProperty(properties, "SDL.window.create.y", centered));

            return SDL3.ThrowIfNull(SDL3.CreateWindowWithProperties(properties));
        }
        finally
        {
            SDL3.DestroyProperties(properties);
        }
    }

    private static long ToState(WindowOptions options)
    {
        SDL_WindowStates states = SDL_WindowStates.Hidden | SDL_WindowStates.HighPixelDensity;

        if (options.Fullscreen)
            states |= SDL_WindowStates.Fullscreen;

        if (options.Resizable)
            states |= SDL_WindowStates.Resizable;

        if (options.Borderless)
            states |= SDL_WindowStates.Borderless;

        return (long)states;
    }
}
