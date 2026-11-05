// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Numerics;

namespace KappaDuck.Khaos.Windowing;

/// <summary>
/// A screen connected to the computer.
/// </summary>
/// <remarks>
/// <para>
/// Use it to let the player choose the screen the game runs on by listing the screens with <see cref="All"/>.
/// </para>
/// <para>
/// A <see cref="Display"/> stands for a screen only while it stays connected. Unplug a screen and plug it back, and it
/// becomes a new <see cref="Display"/>.
/// </para>
/// <para>
/// In the browser, the game always has a single screen.
/// </para>
/// </remarks>
public readonly struct Display : IEqualityOperators<Display, Display, bool>, IEquatable<Display>
{
    private const int MaxScreens = 16;

    private static readonly Dictionary<uint, string> _names = [];

    internal Display(uint id) => Id = id;

    internal uint Id { get; }

    /// <summary>
    /// Gets a value indicating whether the screen is still connected.
    /// </summary>
    /// <exception cref="PlatformException">The screens could not be read, for example because the game has exited.</exception>
    public bool Connected
    {
        get
        {
            using OwnedBuffer<uint> screens = GetScreens();
            return screens.Span.Contains(Id);
        }
    }

    /// <summary>
    /// Gets a name that tells the screen apart from the others and stays the same from one run to the next.
    /// </summary>
    /// <remarks>
    /// It is the <see cref="Name"/>, unless another connected screen has the same name. Then the screens with that name
    /// are counted from left to right, and the second one is <c>U28E590 #2</c>.
    /// </remarks>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public string Key
    {
        get
        {
            string name = Name;
            RectI bounds = NativeBounds();

            using OwnedBuffer<uint> screens = GetScreens();
            int length = screens.Length;

            Span<RectI> others = length <= MaxScreens ? stackalloc RectI[MaxScreens] : new RectI[length];
            int count = 0;

            foreach (uint id in screens)
            {
                if (id == Id)
                    continue;

                Display display = new(id);

                if (display.Name == name)
                    others[count++] = display.NativeBounds();
            }

            return DisplayKey.Of(name, DisplayKey.Rank(bounds, others[..count]));
        }
    }

    /// <summary>
    /// Gets the name of the screen, as the system shows it, such as <c>U28E590</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two screens of the same model have the same name. To tell them apart use <see cref="Key"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public string Name
    {
        get
        {
            byte* name = SDL3.ThrowIfNull(SDL3.GetDisplayName(Id));
            return _names.TryGetValue(Id, out string? known) ? known : Remember(Id, name);
        }
    }

    /// <summary>
    /// Gets how many times per second the screen refreshes, in hertz, such as 60 or 144. 0 when the system does not
    /// say.
    /// </summary>
    /// <remarks>
    /// With vertical sync, the game draws one frame per refresh.
    /// </remarks>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public float RefreshRate => CurrentMode().RefreshRate;

    /// <summary>
    /// Gets how many real pixels the screen has for each logical unit, which is its scale setting in the system
    /// settings, such as 1 at 100 %, 1.5 at 150 % or 2 at 200 %.
    /// </summary>
    /// <remarks>
    /// A window on this screen has the same <see cref="Window.PixelDensity"/>.
    /// </remarks>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public float Scale => CurrentMode().PixelDensity * ContentScale();

    /// <summary>
    /// Gets the size of the whole screen in logical units, the unit of <see cref="Window.Size"/>.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SizeInPixels"/> for the real resolution.
    /// </remarks>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public SizeI Size => WindowScale.ToLogical(NativeBounds().Size, ContentScale());

    /// <summary>
    /// Gets the resolution of the screen in real pixels, such as 2560 × 1440.
    /// </summary>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public SizeI SizeInPixels => WindowScale.ToPixels(NativeBounds().Size, CurrentMode().PixelDensity);

    /// <summary>
    /// Gets the part of the screen that the taskbar, the dock or a menu bar leaves free, in logical units, from the
    /// top-left corner of the screen.
    /// </summary>
    /// <remarks>
    /// When the system does not say, the area is the whole screen.
    /// </remarks>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public RectI UsableArea => WindowScale.ToLogical(NativeUsableArea(), ContentScale());

    /// <summary>
    /// Gets the same area as <see cref="UsableArea"/>, in real pixels.
    /// </summary>
    /// <exception cref="PlatformException">The screen is no longer connected, or the game has exited.</exception>
    public RectI UsableAreaInPixels => WindowScale.ToPixels(NativeUsableArea(), CurrentMode().PixelDensity);

    /// <summary>
    /// Gets every screen connected right now.
    /// </summary>
    /// <exception cref="PlatformException">The screens could not be read, for example because the game has exited.</exception>
    public static IReadOnlyList<Display> All
    {
        get
        {
            using OwnedBuffer<uint> screens = GetScreens();
            Display[] displays = new Display[screens.Length];

            for (int i = 0; i < screens.Length; i++)
                displays[i] = new Display(screens[i]);

            return displays;
        }
    }

    /// <summary>
    /// Gets the main screen of the computer, the one the player chose as primary in the system settings.
    /// </summary>
    /// <exception cref="PlatformException">The screens could not be read, for example because the game has exited.</exception>
    public static Display Primary => new(SDL3.ThrowIfZero(SDL3.GetPrimaryDisplay()));

    /// <summary>
    /// Finds the connected screen that has this key or this name.
    /// </summary>
    /// <remarks>
    /// The key or the name must match exactly, case included. A name that several screens share gives the leftmost one.
    /// </remarks>
    /// <param name="key">The key of the screen, as <see cref="Key"/> gives it, or its <see cref="Name"/>.</param>
    /// <returns>The screen, or <see langword="null"/> when no connected screen has that key or that name.</returns>
    /// <exception cref="PlatformException">The screens could not be read, for example because the game has exited.</exception>
    public static Display? Find(string key)
    {
        (string name, int rank) = DisplayKey.Parse(key);
        return FromTheLeft(name, rank) ?? (rank > 1 ? FromTheLeft(key, 1) : null);
    }

    /// <inheritdoc/>
    public bool Equals(Display other) => Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Display other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Determines whether two values stand for the same screen.
    /// </summary>
    /// <param name="left">The first screen.</param>
    /// <param name="right">The second screen.</param>
    /// <returns><see langword="true"/> if both stand for the same screen; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Display left, Display right) => left.Equals(right);

    /// <summary>
    /// Determines whether two values stand for different screens.
    /// </summary>
    /// <param name="left">The first screen.</param>
    /// <param name="right">The second screen.</param>
    /// <returns><see langword="true"/> if they stand for different screens; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Display left, Display right) => !left.Equals(right);

    internal float ContentScale() => SDL3.ThrowIfZero(SDL3.GetDisplayContentScale(Id));

    private SDL_DisplayMode CurrentMode()
    {
        SDL_DisplayMode* mode = SDL3.ThrowIfNull(SDL3.GetCurrentDisplayMode(Id));

        // SAFETY: mode is not null and points to the mode SDL keeps for this screen; it is copied before any other call.
        return unsafe (*mode);
    }

    private RectI NativeUsableArea()
    {
        SDL3.ThrowIfFailed(SDL3.GetDisplayUsableBounds(Id, out RectI usable));
        RectI bounds = NativeBounds();

        return new RectI(usable.X - bounds.X, usable.Y - bounds.Y, usable.Width, usable.Height);
    }

    internal RectI NativeBounds()
    {
        SDL3.ThrowIfFailed(SDL3.GetDisplayBounds(Id, out RectI bounds));
        return bounds;
    }

    private static Display? FromTheLeft(string name, int rank)
    {
        using OwnedBuffer<uint> screens = GetScreens();
        int length = screens.Length;

        Span<uint> matches = length <= MaxScreens ? stackalloc uint[MaxScreens] : new uint[length];
        Span<RectI> bounds = length <= MaxScreens ? stackalloc RectI[MaxScreens] : new RectI[length];

        int count = WithName(name, screens.Span, matches, bounds);
        int index = DisplayKey.Pick(rank, bounds[..count]);

        return index < 0 ? null : new Display(matches[index]);
    }

    private static OwnedBuffer<uint> GetScreens() => new(SDL3.ThrowIfNull(SDL3.GetDisplays(out int count)), count);

    private static string Remember(uint id, byte* bytes)
    {
        // SAFETY: bytes is not null and points to the null-terminated name SDL keeps while the screen is connected.
        string name = unsafe (Utf8StringMarshaller.ConvertToManaged(bytes))!;

        _names.Add(id, name);
        return name;
    }

    private static int WithName(string name, ReadOnlySpan<uint> ids, Span<uint> matches, Span<RectI> bounds)
    {
        int count = 0;

        foreach (uint id in ids)
        {
            Display display = new(id);

            if (display.Name != name)
                continue;

            matches[count] = id;
            bounds[count++] = display.NativeBounds();
        }

        return count;
    }
}
