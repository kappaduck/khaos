// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;
using KappaDuck.Khaos.Graphics.Rendering;
using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos;

/// <summary>
/// The base class of a 2D game: Khaos opens its window, then calls its methods once per frame.
/// </summary>
/// <remarks>
/// Override <see cref="Draw"/>, and <see cref="Initialize"/> when the game has something to prepare.
/// </remarks>
public abstract class Game2D
{
    private readonly SizeI _size;
    private readonly string? _title;

    /// <summary>
    /// Creates a game whose window has this size, in logical units, and the name of the game as its title.
    /// </summary>
    /// <param name="width">The width of the inside of the window, in logical units.</param>
    /// <param name="height">The height of the inside of the window, in logical units.</param>
    protected Game2D(int width, int height) => _size = new SizeI(width, height);

    /// <summary>
    /// Creates a game whose window has this title and this size, in logical units.
    /// </summary>
    /// <param name="title">The title of the window. The game keeps its own name, set by <c>Product</c> in its project file.</param>
    /// <param name="width">The width of the inside of the window, in logical units.</param>
    /// <param name="height">The height of the inside of the window, in logical units.</param>
    protected Game2D(string title, int width, int height)
    {
        _title = title;
        _size = new SizeI(width, height);
    }

    /// <summary>
    /// Gets the window the game runs in. Use it from <see cref="Initialize"/> or later.
    /// </summary>
    /// <exception cref="InvalidOperationException">The window is not open yet: the game is still being created.</exception>
    public Window Window
    {
        get => field ?? throw new InvalidOperationException(Window.NotOpenYet);
        internal set;
    }

    internal bool ExitRequested { get; private set; }

    /// <summary>
    /// Closes the game once the current frame ends.
    /// </summary>
    public void Exit() => ExitRequested = true;

    /// <summary>
    /// Draws the frame. Khaos calls it once per frame, and shows what you drew when it returns.
    /// </summary>
    /// <param name="renderer">The renderer to draw the frame with.</param>
    protected internal abstract void Draw(Renderer renderer);

    /// <summary>
    /// Prepares the game. Khaos calls it once, after the window opens and before the first frame.
    /// </summary>
    protected internal virtual void Initialize()
    {
    }

    /// <summary>
    /// Reacts to something that happened, such as a key pressed. Khaos calls it once per event, before the frame
    /// draws.
    /// </summary>
    /// <param name="e">The event. Match it with a <see langword="switch"/> or <see langword="is"/>.</param>
    protected internal virtual void OnEvent(in Event e)
    {
    }

    /// <summary>
    /// Ends the game. Khaos calls it once, after the last frame and before the window closes, when
    /// <see cref="Initialize"/> has run.
    /// </summary>
    protected internal virtual void Shutdown()
    {
    }

    internal void ApplyTo(WindowOptions options)
    {
        if (_title is not null)
            options.Title = _title;

        options.Width = _size.Width;
        options.Height = _size.Height;
    }

    internal void Reset() => ExitRequested = false;
}
