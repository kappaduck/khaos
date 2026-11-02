// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Collections;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Enumerates the cells of a <see cref="RectI"/>, row by row from the top-left one.
/// </summary>
public ref struct CellEnumerator : IEnumerator<PointI>
{
    private readonly int _left;
    private readonly int _top;
    private readonly int _right;
    private readonly int _bottom;

    private int _x;
    private int _y;

    internal CellEnumerator(RectI rect)
    {
        _left = rect.Left;
        _top = rect.Top;
        _right = rect.Right;
        _bottom = rect.IsEmpty ? rect.Top : rect.Bottom;

        _x = _left - 1;
        _y = _top;
    }

    /// <summary>
    /// Gets the current cell.
    /// </summary>
    public readonly PointI Current => new(_x, _y);

    /// <inheritdoc/>
    readonly object IEnumerator.Current => Current;

    /// <summary>
    /// Returns this enumerator.
    /// </summary>
    /// <returns>This enumerator.</returns>
    public readonly CellEnumerator GetEnumerator() => this;

    /// <summary>
    /// Advances to the next cell.
    /// </summary>
    /// <returns><see langword="true"/> if there is a next cell; otherwise, <see langword="false"/>.</returns>
    public bool MoveNext()
    {
        if (_y >= _bottom)
            return false;

        if (++_x < _right)
            return true;

        _x = _left;
        return ++_y < _bottom;
    }

    /// <summary>
    /// Moves back to before the first cell, so the next <see cref="MoveNext"/> starts over.
    /// </summary>
    public void Reset()
    {
        _x = _left - 1;
        _y = _top;
    }

    /// <inheritdoc/>
    public readonly void Dispose()
    {
    }
}
