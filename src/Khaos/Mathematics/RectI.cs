// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents an axis-aligned rectangle of whole cells, such as pixels or tiles, from its top-left corner.
/// </summary>
/// <param name="x">The x-coordinate of the left edge.</param>
/// <param name="y">The y-coordinate of the top edge.</param>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly struct RectI(int x, int y, int width, int height) :
    IEqualityOperators<RectI, RectI, bool>,
    IEquatable<RectI>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RectI"/> struct from its top-left corner and its size.
    /// </summary>
    /// <param name="position">The top-left corner.</param>
    /// <param name="size">The size.</param>
    public RectI(PointI position, SizeI size) : this(position.X, position.Y, size.Width, size.Height)
    {
    }

    /// <summary>
    /// Gets the rectangle (0, 0, 0, 0).
    /// </summary>
    public static RectI Zero { get; }

    /// <summary>
    /// Gets the x-coordinate of the left edge.
    /// </summary>
    public int X { get; init; } = x;

    /// <summary>
    /// Gets the y-coordinate of the top edge.
    /// </summary>
    public int Y { get; init; } = y;

    /// <summary>
    /// Gets the width.
    /// </summary>
    public int Width { get; init; } = width;

    /// <summary>
    /// Gets the height.
    /// </summary>
    public int Height { get; init; } = height;

    /// <summary>
    /// Gets the number of cells covered.
    /// </summary>
    public long Area => (long)Width * Height;

    /// <summary>
    /// Gets a value indicating whether the width or the height is zero or negative.
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Gets the top-left corner.
    /// </summary>
    public PointI Position => new(X, Y);

    /// <summary>
    /// Gets the size.
    /// </summary>
    public SizeI Size => new(Width, Height);

    /// <summary>
    /// Gets the x-coordinate of the left edge, the first column inside. Same as <see cref="X"/>.
    /// </summary>
    public int Left => X;

    /// <summary>
    /// Gets the y-coordinate of the top edge, the first row inside. Same as <see cref="Y"/>.
    /// </summary>
    public int Top => Y;

    /// <summary>
    /// Gets the x-coordinate of the right edge, the first column outside.
    /// </summary>
    public int Right => X + Width;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge, the first row outside.
    /// </summary>
    public int Bottom => Y + Height;

    /// <summary>
    /// Gets the center.
    /// </summary>
    /// <remarks>
    /// A <see cref="Point"/>, because the center of a rectangle with an odd size falls between two cells.
    /// </remarks>
    public Point Center => new(X + (Width * 0.5f), Y + (Height * 0.5f));

    /// <summary>
    /// Gets every cell inside the rectangle, row by row from the top-left one.
    /// </summary>
    /// <remarks>
    /// The rectangle covers the cells from <see cref="Left"/> up to, but not including, <see cref="Right"/>.
    /// A 10 × 10 rectangle at the origin covers x from 0 to 9, so (10, 0) is outside, and two rectangles
    /// sharing an edge do not intersect.
    /// </remarks>
    public CellEnumerator Cells => new(this);

    /// <summary>
    /// Creates a rectangle from the coordinates of its edges.
    /// </summary>
    /// <param name="left">The x-coordinate of the left edge, the first column inside.</param>
    /// <param name="top">The y-coordinate of the top edge, the first row inside.</param>
    /// <param name="right">The x-coordinate of the right edge, the first column outside.</param>
    /// <param name="bottom">The y-coordinate of the bottom edge, the first row outside.</param>
    /// <returns>The rectangle.</returns>
    public static RectI FromEdges(int left, int top, int right, int bottom) => new(left, top, right - left, bottom - top);

    /// <summary>
    /// Computes the smallest rectangle containing a set of cells.
    /// </summary>
    /// <remarks>
    /// A single cell gives a 1 × 1 rectangle, because the rectangle must cover that cell.
    /// </remarks>
    /// <param name="points">The cells to enclose.</param>
    /// <param name="bounds">When this method returns <see langword="true"/>, the enclosing rectangle; otherwise, <see cref="Zero"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="points"/> is not empty; otherwise, <see langword="false"/>.</returns>
    public static bool TryEnclose(ReadOnlySpan<PointI> points, out RectI bounds)
    {
        if (points.IsEmpty)
        {
            bounds = Zero;
            return false;
        }

        int left = points[0].X;
        int top = points[0].Y;
        int right = left;
        int bottom = top;

        foreach (PointI point in points[1..])
        {
            left = Math.Min(left, point.X);
            top = Math.Min(top, point.Y);
            right = Math.Max(right, point.X);
            bottom = Math.Max(bottom, point.Y);
        }

        bounds = FromEdges(left, top, right + 1, bottom + 1);
        return true;
    }

    /// <summary>
    /// Computes the smallest rectangle containing a sequence of points.
    /// </summary>
    /// <remarks>
    /// Same result as <see cref="TryEnclose(ReadOnlySpan{PointI}, out RectI)"/>. Arrays and <see cref="List{T}"/>
    /// are read without enumerating them; enumerating any other sequence allocates.
    /// </remarks>
    /// <param name="points">The points to enclose.</param>
    /// <param name="bounds">When this method returns <see langword="true"/>, the enclosing rectangle; otherwise, <see cref="Zero"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="points"/> is not empty; otherwise, <see langword="false"/>.</returns>
    public static bool TryEnclose(IEnumerable<PointI> points, out RectI bounds)
    {
        if (TryGetSpan(points, out ReadOnlySpan<PointI> span))
            return TryEnclose(span, out bounds);

        using IEnumerator<PointI> enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            bounds = Zero;
            return false;
        }

        bounds = new RectI(enumerator.Current.X, enumerator.Current.Y, 1, 1);

        while (enumerator.MoveNext())
            bounds = Union(bounds, enumerator.Current);

        return true;
    }

    /// <summary>
    /// Computes the intersection of two rectangles.
    /// </summary>
    /// <remarks>
    /// Rectangles sharing only an edge do not intersect.
    /// </remarks>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <param name="intersection">When this method returns <see langword="true"/>, the intersection; otherwise, <see cref="Zero"/>.</param>
    /// <returns><see langword="true"/> if the rectangles intersect; otherwise, <see langword="false"/>.</returns>
    public static bool TryIntersect(RectI left, RectI right, out RectI intersection)
    {
        RectI result = FromEdges(
            Math.Max(left.Left, right.Left),
            Math.Max(left.Top, right.Top),
            Math.Min(left.Right, right.Right),
            Math.Min(left.Bottom, right.Bottom));

        intersection = result.IsEmpty ? Zero : result;
        return !result.IsEmpty;
    }

    /// <summary>
    /// Computes the smallest rectangle containing two rectangles.
    /// </summary>
    /// <remarks>
    /// An empty rectangle is ignored, and the union of two empty rectangles is <see cref="Zero"/>.
    /// </remarks>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns>The union of the two rectangles.</returns>
    public static RectI Union(RectI left, RectI right)
    {
        if (left.IsEmpty)
            return right.IsEmpty ? Zero : right;

        if (right.IsEmpty)
            return left;

        return FromEdges(
            Math.Min(left.Left, right.Left),
            Math.Min(left.Top, right.Top),
            Math.Max(left.Right, right.Right),
            Math.Max(left.Bottom, right.Bottom));
    }

    /// <summary>
    /// Computes the smallest rectangle containing a rectangle and a cell.
    /// </summary>
    /// <remarks>
    /// Grows a bounding box one cell at a time. An empty rectangle is ignored, so the result is then the
    /// 1 × 1 rectangle covering the cell.
    /// </remarks>
    /// <param name="rect">The rectangle.</param>
    /// <param name="point">The cell.</param>
    /// <returns>The union of the rectangle and the cell.</returns>
    public static RectI Union(RectI rect, PointI point) => Union(rect, new RectI(point.X, point.Y, 1, 1));

    /// <summary>
    /// Determines whether a cell is inside the rectangle.
    /// </summary>
    /// <remarks>
    /// The right and bottom edges are outside.
    /// </remarks>
    /// <param name="point">The cell to test.</param>
    /// <returns><see langword="true"/> if the cell is inside; otherwise, <see langword="false"/>.</returns>
    public bool Contains(PointI point) => point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;

    /// <summary>
    /// Determines whether another rectangle is entirely inside this one.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns><see langword="true"/> if neither rectangle is empty and every cell of <paramref name="other"/> is inside; otherwise, <see langword="false"/>.</returns>
    public bool Contains(RectI other)
    {
        return !IsEmpty && !other.IsEmpty
            && other.Left >= Left && other.Right <= Right
            && other.Top >= Top && other.Bottom <= Bottom;
    }

    /// <summary>
    /// Determines whether every cell is inside the rectangle.
    /// </summary>
    /// <param name="points">The cells to test.</param>
    /// <returns><see langword="true"/> if every cell is inside, or if there is no cell; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAll(ReadOnlySpan<PointI> points)
    {
        foreach (PointI point in points)
        {
            if (!Contains(point))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether every cell is inside the rectangle.
    /// </summary>
    /// <remarks>
    /// Arrays and <see cref="List{T}"/> are read without enumerating them. Prefer the
    /// <see cref="ContainsAll(ReadOnlySpan{PointI})"/> overload in code that runs every frame: enumerating any
    /// other sequence allocates.
    /// </remarks>
    /// <param name="points">The points to test.</param>
    /// <returns><see langword="true"/> if every cell is inside, or if there is no cell; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAll(IEnumerable<PointI> points)
        => TryGetSpan(points, out ReadOnlySpan<PointI> span) ? ContainsAll(span) : points.All(Contains);

    /// <summary>
    /// Determines whether at least one cell is inside the rectangle.
    /// </summary>
    /// <param name="points">The cells to test.</param>
    /// <returns><see langword="true"/> if at least one cell is inside; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAny(ReadOnlySpan<PointI> points)
    {
        foreach (PointI point in points)
        {
            if (Contains(point))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether at least one cell is inside the rectangle.
    /// </summary>
    /// <remarks>
    /// Arrays and <see cref="List{T}"/> are read without enumerating them. Prefer the
    /// <see cref="ContainsAny(ReadOnlySpan{PointI})"/> overload in code that runs every frame: enumerating any
    /// other sequence allocates.
    /// </remarks>
    /// <param name="points">The points to test.</param>
    /// <returns><see langword="true"/> if at least one cell is inside; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAny(IEnumerable<PointI> points)
        => TryGetSpan(points, out ReadOnlySpan<PointI> span) ? ContainsAny(span) : points.Any(Contains);

    /// <summary>
    /// Determines whether this rectangle and another share at least one cell.
    /// </summary>
    /// <remarks>
    /// Rectangles sharing only an edge do not intersect.
    /// </remarks>
    /// <param name="other">The other rectangle.</param>
    /// <returns><see langword="true"/> if the rectangles intersect; otherwise, <see langword="false"/>.</returns>
    public bool Intersects(RectI other)
    {
        return Math.Min(Right, other.Right) > Math.Max(Left, other.Left)
            && Math.Min(Bottom, other.Bottom) > Math.Max(Top, other.Top);
    }

    /// <summary>
    /// Returns this rectangle moved by a whole number of cells on each axis.
    /// </summary>
    /// <param name="dx">The horizontal offset.</param>
    /// <param name="dy">The vertical offset.</param>
    /// <returns>The moved rectangle.</returns>
    public RectI Offset(int dx, int dy) => new(X + dx, Y + dy, Width, Height);

    /// <summary>
    /// Deconstructs the rectangle into its components.
    /// </summary>
    /// <param name="x">The x-coordinate of the left edge.</param>
    /// <param name="y">The y-coordinate of the top edge.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }

    /// <inheritdoc/>
    public bool Equals(RectI other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is RectI other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    /// <summary>
    /// Returns the rectangle as <c>(x, y, width, height)</c>, using the current culture.
    /// </summary>
    /// <returns>The formatted rectangle.</returns>
    public override string ToString() => ToString(null, null);

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => GeometryFormatting.ToString(this, format, formatProvider);

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => GeometryFormatting.TryFormat(destination, out charsWritten, [X, Y, Width, Height], format, provider);

    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => GeometryFormatting.TryFormat(utf8Destination, out bytesWritten, [X, Y, Width, Height], format, provider);

    private static bool TryGetSpan(IEnumerable<PointI> points, out ReadOnlySpan<PointI> span)
    {
        if (points is PointI[] array)
        {
            span = array;
            return true;
        }

        if (points is List<PointI> list)
        {
            span = CollectionsMarshal.AsSpan(list);
            return true;
        }

        span = default;
        return false;
    }

    /// <summary>
    /// Determines whether two rectangles are equal.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns><see langword="true"/> if the rectangles are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(RectI left, RectI right) => left.Equals(right);

    /// <summary>
    /// Determines whether two rectangles differ.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns><see langword="true"/> if the rectangles differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(RectI left, RectI right) => !left.Equals(right);

    /// <summary>
    /// Converts an integer rectangle to a floating-point rectangle with the same edges.
    /// </summary>
    /// <param name="rect">The rectangle to convert.</param>
    public static implicit operator Rect(RectI rect) => new(rect.X, rect.Y, rect.Width, rect.Height);
}
