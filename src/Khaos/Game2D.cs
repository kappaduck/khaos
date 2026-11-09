// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using KappaDuck.Khaos.Graphics.Rendering;
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
    /// <summary>
    /// Creates a game whose window has this size, in logical units, and the name of the game as its title.
    /// </summary>
    /// <param name="width">The width of the inside of the window, in logical units.</param>
    /// <param name="height">The height of the inside of the window, in logical units.</param>
    protected Game2D(int width, int height)
        => WindowOptions = new WindowOptions { Width = width, Height = height };

    /// <summary>
    /// Creates a game whose window has this title and this size, in logical units.
    /// </summary>
    /// <param name="title">The title of the window. The game keeps its own name, set by <c>Product</c> in its project file.</param>
    /// <param name="width">The width of the inside of the window, in logical units.</param>
    /// <param name="height">The height of the inside of the window, in logical units.</param>
    protected Game2D(string title, int width, int height)
        => WindowOptions = new WindowOptions { Title = title, Width = width, Height = height };

    internal bool ExitRequested { get; private set; }

    internal GraphicsOptions GraphicsOptions { get; } = new();

    internal WindowOptions WindowOptions { get; }

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

    internal void Reset() => ExitRequested = false;
}
