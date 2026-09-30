// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents an axis-aligned rectangle in floating-point coordinates, from its top-left corner.
/// </summary>
/// <param name="x">The x-coordinate of the left edge.</param>
/// <param name="y">The y-coordinate of the top edge.</param>
/// <param name="width">The width.</param>
/// <param name="height">The height.</param>
[StructLayout(LayoutKind.Sequential)]
public readonly struct Rect(float x, float y, float width, float height) :
    IEqualityOperators<Rect, Rect, bool>,
    IEquatable<Rect>,
    ISpanFormattable,
    IUtf8SpanFormattable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct from its top-left corner and its size.
    /// </summary>
    /// <param name="position">The top-left corner.</param>
    /// <param name="size">The size.</param>
    public Rect(Point position, Size size) : this(position.X, position.Y, size.Width, size.Height)
    {
    }

    /// <summary>
    /// Gets the rectangle (0, 0, 0, 0).
    /// </summary>
    public static Rect Zero { get; }

    /// <summary>
    /// Gets the x-coordinate of the left edge.
    /// </summary>
    public float X { get; init; } = x;

    /// <summary>
    /// Gets the y-coordinate of the top edge.
    /// </summary>
    public float Y { get; init; } = y;

    /// <summary>
    /// Gets the width.
    /// </summary>
    public float Width { get; init; } = width;

    /// <summary>
    /// Gets the height.
    /// </summary>
    public float Height { get; init; } = height;

    /// <summary>
    /// Gets the area.
    /// </summary>
    public float Area => Width * Height;

    /// <summary>
    /// Gets a value indicating whether the width or the height is negative.
    /// </summary>
    public bool IsEmpty => Width < 0f || Height < 0f;

    /// <summary>
    /// Gets the top-left corner.
    /// </summary>
    public Point Position => new(X, Y);

    /// <summary>
    /// Gets the size.
    /// </summary>
    public Size Size => new(Width, Height);

    /// <summary>
    /// Gets the x-coordinate of the left edge. Same as <see cref="X"/>.
    /// </summary>
    public float Left => X;

    /// <summary>
    /// Gets the y-coordinate of the top edge. Same as <see cref="Y"/>.
    /// </summary>
    public float Top => Y;

    /// <summary>
    /// Gets the x-coordinate of the right edge.
    /// </summary>
    public float Right => X + Width;

    /// <summary>
    /// Gets the y-coordinate of the bottom edge.
    /// </summary>
    public float Bottom => Y + Height;

    /// <summary>
    /// Gets the center.
    /// </summary>
    public Point Center => new(X + (Width * 0.5f), Y + (Height * 0.5f));

    /// <summary>
    /// Gets the top-left corner.
    /// </summary>
    public Point TopLeft => new(Left, Top);

    /// <summary>
    /// Gets the top-right corner.
    /// </summary>
    public Point TopRight => new(Right, Top);

    /// <summary>
    /// Gets the bottom-left corner.
    /// </summary>
    public Point BottomLeft => new(Left, Bottom);

    /// <summary>
    /// Gets the bottom-right corner.
    /// </summary>
    public Point BottomRight => new(Right, Bottom);

    /// <summary>
    /// Creates a rectangle from the coordinates of its edges.
    /// </summary>
    /// <param name="left">The x-coordinate of the left edge.</param>
    /// <param name="top">The y-coordinate of the top edge.</param>
    /// <param name="right">The x-coordinate of the right edge.</param>
    /// <param name="bottom">The y-coordinate of the bottom edge.</param>
    /// <returns>The rectangle.</returns>
    public static Rect FromEdges(float left, float top, float right, float bottom) => new(left, top, right - left, bottom - top);

    /// <summary>
    /// Computes the smallest rectangle containing a set of points.
    /// </summary>
    /// <remarks>
    /// A single point gives a rectangle of size zero at that point.
    /// </remarks>
    /// <param name="points">The points to enclose.</param>
    /// <param name="bounds">When this method returns <see langword="true"/>, the enclosing rectangle; otherwise, <see cref="Zero"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="points"/> is not empty; otherwise, <see langword="false"/>.</returns>
    public static bool TryEnclose(ReadOnlySpan<Point> points, out Rect bounds)
    {
        if (points.IsEmpty)
        {
            bounds = Zero;
            return false;
        }

        float left = points[0].X;
        float top = points[0].Y;
        float right = left;
        float bottom = top;

        foreach (Point point in points[1..])
        {
            left = MathF.Min(left, point.X);
            top = MathF.Min(top, point.Y);
            right = MathF.Max(right, point.X);
            bottom = MathF.Max(bottom, point.Y);
        }

        bounds = FromEdges(left, top, right, bottom);
        return true;
    }

    /// <summary>
    /// Computes the smallest rectangle containing a sequence of points.
    /// </summary>
    /// <remarks>
    /// Same result as <see cref="TryEnclose(ReadOnlySpan{Point}, out Rect)"/>. Arrays and <see cref="List{T}"/>
    /// are read without enumerating them; enumerating any other sequence allocates.
    /// </remarks>
    /// <param name="points">The points to enclose.</param>
    /// <param name="bounds">When this method returns <see langword="true"/>, the enclosing rectangle; otherwise, <see cref="Zero"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="points"/> is not empty; otherwise, <see langword="false"/>.</returns>
    public static bool TryEnclose(IEnumerable<Point> points, out Rect bounds)
    {
        if (TryGetSpan(points, out ReadOnlySpan<Point> span))
            return TryEnclose(span, out bounds);

        using IEnumerator<Point> enumerator = points.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            bounds = Zero;
            return false;
        }

        bounds = new Rect(enumerator.Current, Size.Zero);

        while (enumerator.MoveNext())
            bounds = Union(bounds, enumerator.Current);

        return true;
    }

    /// <summary>
    /// Computes the intersection of two rectangles.
    /// </summary>
    /// <remarks>
    /// Rectangles sharing only an edge intersect, with a result of zero width or height.
    /// </remarks>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <param name="intersection">When this method returns <see langword="true"/>, the intersection; otherwise, <see cref="Zero"/>.</param>
    /// <returns><see langword="true"/> if the rectangles intersect; otherwise, <see langword="false"/>.</returns>
    public static bool TryIntersect(Rect left, Rect right, out Rect intersection)
    {
        Rect result = FromEdges(
            MathF.Max(left.Left, right.Left),
            MathF.Max(left.Top, right.Top),
            MathF.Min(left.Right, right.Right),
            MathF.Min(left.Bottom, right.Bottom));

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
    public static Rect Union(Rect left, Rect right)
    {
        if (left.IsEmpty)
            return right.IsEmpty ? Zero : right;

        if (right.IsEmpty)
            return left;

        return FromEdges(
            MathF.Min(left.Left, right.Left),
            MathF.Min(left.Top, right.Top),
            MathF.Max(left.Right, right.Right),
            MathF.Max(left.Bottom, right.Bottom));
    }

    /// <summary>
    /// Computes the smallest rectangle containing a rectangle and a point.
    /// </summary>
    /// <remarks>
    /// Grows a bounding box one point at a time. An empty rectangle is ignored, so the result is then the
    /// point itself, with a size of zero.
    /// </remarks>
    /// <param name="rect">The rectangle.</param>
    /// <param name="point">The point.</param>
    /// <returns>The union of the rectangle and the point.</returns>
    public static Rect Union(Rect rect, Point point)
    {
        if (rect.IsEmpty)
            return new Rect(point, Size.Zero);

        return FromEdges(
            MathF.Min(rect.Left, point.X),
            MathF.Min(rect.Top, point.Y),
            MathF.Max(rect.Right, point.X),
            MathF.Max(rect.Bottom, point.Y));
    }

    /// <summary>
    /// Determines whether a point is inside the rectangle, edges included.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns><see langword="true"/> if the point is inside or on an edge; otherwise, <see langword="false"/>.</returns>
    public bool Contains(Point point) => point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom;

    /// <summary>
    /// Determines whether another rectangle is entirely inside this one, edges included.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns><see langword="true"/> if neither rectangle is empty and <paramref name="other"/> is inside; otherwise, <see langword="false"/>.</returns>
    public bool Contains(Rect other)
    {
        return !IsEmpty && !other.IsEmpty
            && other.Left >= Left && other.Right <= Right
            && other.Top >= Top && other.Bottom <= Bottom;
    }

    /// <summary>
    /// Determines whether every point is inside the rectangle.
    /// </summary>
    /// <param name="points">The points to test.</param>
    /// <returns><see langword="true"/> if every point is inside, or if there is no point; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAll(ReadOnlySpan<Point> points)
    {
        foreach (Point point in points)
        {
            if (!Contains(point))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether every point is inside the rectangle.
    /// </summary>
    /// <remarks>
    /// Arrays and <see cref="List{T}"/> are read without enumerating them. Prefer the
    /// <see cref="ContainsAll(ReadOnlySpan{Point})"/> overload in code that runs every frame: enumerating any
    /// other sequence allocates.
    /// </remarks>
    /// <param name="points">The points to test.</param>
    /// <returns><see langword="true"/> if every point is inside, or if there is no point; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAll(IEnumerable<Point> points)
        => TryGetSpan(points, out ReadOnlySpan<Point> span) ? ContainsAll(span) : points.All(Contains);

    /// <summary>
    /// Determines whether at least one point is inside the rectangle.
    /// </summary>
    /// <param name="points">The points to test.</param>
    /// <returns><see langword="true"/> if at least one point is inside; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAny(ReadOnlySpan<Point> points)
    {
        foreach (Point point in points)
        {
            if (Contains(point))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether at least one point is inside the rectangle.
    /// </summary>
    /// <remarks>
    /// Arrays and <see cref="List{T}"/> are read without enumerating them. Prefer the
    /// <see cref="ContainsAny(ReadOnlySpan{Point})"/> overload in code that runs every frame: enumerating any
    /// other sequence allocates.
    /// </remarks>
    /// <param name="points">The points to test.</param>
    /// <returns><see langword="true"/> if at least one point is inside; otherwise, <see langword="false"/>.</returns>
    public bool ContainsAny(IEnumerable<Point> points)
        => TryGetSpan(points, out ReadOnlySpan<Point> span) ? ContainsAny(span) : points.Any(Contains);

    /// <summary>
    /// Determines whether this rectangle and another share at least one point.
    /// </summary>
    /// <remarks>
    /// Rectangles sharing only an edge intersect.
    /// </remarks>
    /// <param name="other">The other rectangle.</param>
    /// <returns><see langword="true"/> if the rectangles intersect; otherwise, <see langword="false"/>.</returns>
    public bool Intersects(Rect other)
    {
        return MathF.Min(Right, other.Right) >= MathF.Max(Left, other.Left)
            && MathF.Min(Bottom, other.Bottom) >= MathF.Max(Top, other.Top);
    }

    /// <summary>
    /// Returns this rectangle moved by a displacement.
    /// </summary>
    /// <param name="displacement">The displacement.</param>
    /// <returns>The moved rectangle.</returns>
    public Rect Offset(Vector2 displacement) => new(X + displacement.X, Y + displacement.Y, Width, Height);

    /// <summary>
    /// Returns the integer rectangle whose edges are this rectangle's edges rounded to the nearest integer.
    /// </summary>
    /// <remarks>
    /// Rounds the edges, not the position and the size separately, so two rectangles sharing an edge still
    /// share it once rounded. Halves go away from zero.
    /// </remarks>
    /// <returns>The rounded rectangle.</returns>
    public RectI Round()
    {
        return RectI.FromEdges(
            RoundToInt(Left),
            RoundToInt(Top),
            RoundToInt(Right),
            RoundToInt(Bottom));

        static int RoundToInt(float value) => (int)MathF.Round(value, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Returns the smallest integer rectangle that covers this rectangle entirely.
    /// </summary>
    /// <remarks>
    /// The left and top edges are rounded down, the right and bottom edges up. Use it to find every pixel or
    /// tile a rectangle touches, such as the tiles visible through a camera.
    /// </remarks>
    /// <returns>The covering rectangle.</returns>
    public RectI RoundOut()
    {
        return RectI.FromEdges(
            (int)MathF.Floor(Left),
            (int)MathF.Floor(Top),
            (int)MathF.Ceiling(Right),
            (int)MathF.Ceiling(Bottom));
    }

    /// <summary>
    /// Determines whether this rectangle is within <paramref name="tolerance"/> of another on every component.
    /// </summary>
    /// <param name="other">The rectangle to compare with.</param>
    /// <param name="tolerance">The largest difference allowed on each of x, y, width and height.</param>
    /// <returns><see langword="true"/> if every component is within <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
    public bool IsApproximately(Rect other, float tolerance)
    {
        return MathF.Abs(X - other.X) <= tolerance
            && MathF.Abs(Y - other.Y) <= tolerance
            && MathF.Abs(Width - other.Width) <= tolerance
            && MathF.Abs(Height - other.Height) <= tolerance;
    }

    /// <summary>
    /// Deconstructs the rectangle into its components.
    /// </summary>
    /// <param name="x">The x-coordinate of the left edge.</param>
    /// <param name="y">The y-coordinate of the top edge.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void Deconstruct(out float x, out float y, out float width, out float height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The comparison is exact. To compare computed values with a tolerance, use
    /// <see cref="IsApproximately(Rect, float)"/>.
    /// </remarks>
#pragma warning disable S1244 // Exact equality is the contract; IsApproximately is the tolerant comparison.
    public bool Equals(Rect other)
        => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
#pragma warning restore S1244

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Rect other && Equals(other);

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

    private static bool TryGetSpan(IEnumerable<Point> points, out ReadOnlySpan<Point> span)
    {
        if (points is Point[] array)
        {
            span = array;
            return true;
        }

        if (points is List<Point> list)
        {
            span = CollectionsMarshal.AsSpan(list);
            return true;
        }

        span = default;
        return false;
    }

    /// <summary>
    /// Determines whether two rectangles are exactly equal.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns><see langword="true"/> if the rectangles are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Rect left, Rect right) => left.Equals(right);

    /// <summary>
    /// Determines whether two rectangles differ.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns><see langword="true"/> if the rectangles differ; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Rect left, Rect right) => !left.Equals(right);
}
