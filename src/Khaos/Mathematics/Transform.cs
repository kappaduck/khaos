// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Represents a 2D affine transform: any combination of translation, rotation, scale and shear.
/// </summary>
/// <remarks>
/// <para>
/// Transforms combine with <c>*</c> and apply <b>right to left</b>: in <c>a * b</c>, <c>b</c> is applied to a
/// point first, then <c>a</c>.
/// </para>
/// <para>
/// Positive rotations turn clockwise on screen, because the Y axis points down.
/// </para>
/// <para>
/// <c>default(Transform)</c> is the zero matrix, which collapses every point onto the origin, not the
/// identity. Start from <see cref="Identity"/>.
/// </para>
/// </remarks>
public readonly struct Transform(Matrix3x2 matrix) :
    IMultiplyOperators<Transform, Transform, Transform>,
    IEqualityOperators<Transform, Transform, bool>,
    IEquatable<Transform>
{
    private const float ShearTolerance = 1e-4f;

    /// <summary>
    /// Gets the transform that leaves every point unchanged.
    /// </summary>
    public static Transform Identity { get; } = new(Matrix3x2.Identity);

    /// <summary>
    /// Gets the matrix of the transform.
    /// </summary>
    public Matrix3x2 Matrix { get; } = matrix;

    /// <summary>
    /// Creates a transform that moves points by a displacement.
    /// </summary>
    /// <param name="displacement">The displacement.</param>
    /// <returns>A translation.</returns>
    public static Transform Translation(Vector2 displacement) => new(Matrix3x2.CreateTranslation(displacement));

    /// <summary>
    /// Creates a transform that scales points away from the origin.
    /// </summary>
    /// <param name="scale">The horizontal and vertical scale factors. A negative factor mirrors that axis.</param>
    /// <returns>A scaling.</returns>
    public static Transform Scaling(Vector2 scale) => new(Matrix3x2.CreateScale(scale));

    /// <summary>
    /// Creates a transform that scales points away from the origin, by the same factor on both axes.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    /// <returns>A uniform scaling.</returns>
    public static Transform Scaling(float scale) => new(Matrix3x2.CreateScale(scale));

    /// <summary>
    /// Creates a transform that rotates points around the origin.
    /// </summary>
    /// <param name="rotation">The rotation, clockwise on screen.</param>
    /// <returns>A rotation.</returns>
    public static Transform Rotation(Angle rotation) => new(Matrix3x2.CreateRotation(rotation.Radians));

    /// <summary>
    /// Creates a transform that rotates points around a center.
    /// </summary>
    /// <param name="rotation">The rotation, clockwise on screen.</param>
    /// <param name="center">The point that stays in place.</param>
    /// <returns>A rotation around <paramref name="center"/>.</returns>
    public static Transform Rotation(Angle rotation, Point center) => new(Matrix3x2.CreateRotation(rotation.Radians, (Vector2)center));

    /// <summary>
    /// Creates the transform that places an object: scaled, then rotated, then moved to its position, with
    /// <paramref name="origin"/> as the pivot of both the scale and the rotation.
    /// </summary>
    /// <param name="position">Where the object's origin lands.</param>
    /// <param name="rotation">The rotation, clockwise on screen.</param>
    /// <param name="scale">The horizontal and vertical scale factors.</param>
    /// <param name="origin">The pivot, in the object's own coordinates. The point that lands exactly on <paramref name="position"/>.</param>
    /// <returns>The transform from the object's coordinates to the target coordinates.</returns>
    public static Transform Create(Point position, Angle rotation, Vector2 scale, Point origin = default)
    {
        return new(Matrix3x2.CreateTranslation(-(Vector2)origin)
            * Matrix3x2.CreateScale(scale)
            * Matrix3x2.CreateRotation(rotation.Radians)
            * Matrix3x2.CreateTranslation((Vector2)position));
    }

    /// <summary>
    /// Applies this transform to a point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>The transformed point.</returns>
    public Point TransformPoint(Point point) => (Point)Vector2.Transform((Vector2)point, Matrix);

    /// <summary>
    /// Applies this transform to a displacement, ignoring the translation.
    /// </summary>
    /// <remarks>
    /// A displacement has no position, so moving it does nothing: only the rotation, scale and shear apply.
    /// Use it for velocities and directions.
    /// </remarks>
    /// <param name="vector">The displacement.</param>
    /// <returns>The transformed displacement.</returns>
    public Vector2 TransformVector(Vector2 vector) => Vector2.TransformNormal(vector, Matrix);

    /// <summary>
    /// Computes the smallest axis-aligned rectangle containing a rectangle after this transform.
    /// </summary>
    /// <param name="rect">The rectangle.</param>
    /// <returns>
    /// The bounding rectangle of the four transformed corners. With a rotation or a shear it is larger than the
    /// transformed shape it contains.
    /// </returns>
    public Rect TransformRect(Rect rect)
    {
        Point topLeft = TransformPoint(rect.TopLeft);
        Point topRight = TransformPoint(rect.TopRight);
        Point bottomLeft = TransformPoint(rect.BottomLeft);
        Point bottomRight = TransformPoint(rect.BottomRight);

        return Rect.FromEdges(
            MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomLeft.X, bottomRight.X)),
            MathF.Min(MathF.Min(topLeft.Y, topRight.Y), MathF.Min(bottomLeft.Y, bottomRight.Y)),
            MathF.Max(MathF.Max(topLeft.X, topRight.X), MathF.Max(bottomLeft.X, bottomRight.X)),
            MathF.Max(MathF.Max(topLeft.Y, topRight.Y), MathF.Max(bottomLeft.Y, bottomRight.Y)));
    }

    /// <summary>
    /// Computes the transform that undoes this one.
    /// </summary>
    /// <remarks>
    /// Fails when the transform flattens space, such as a scale of zero on an axis: many points land on the
    /// same spot, so there is no way back.
    /// </remarks>
    /// <param name="inverse">When this method returns <see langword="true"/>, the inverse; otherwise, <see cref="Identity"/>.</param>
    /// <returns><see langword="true"/> if the transform can be undone; otherwise, <see langword="false"/>.</returns>
    public bool TryInvert(out Transform inverse)
    {
        if (Matrix3x2.Invert(Matrix, out Matrix3x2 result))
        {
            inverse = new(result);
            return true;
        }

        inverse = Identity;
        return false;
    }

    /// <summary>
    /// Splits this transform into a translation, a rotation and a scale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Any transform from <see cref="Create"/>, <see cref="Translation"/>, a single <see cref="Rotation(Angle)"/>
    /// or <see cref="Scaling(Vector2)"/> decomposes. A product of transforms may not: a non-uniform scale applied
    /// <b>after</b> a rotation, such as <c>Scaling(new(2, 1)) * Rotation(angle)</c>, shears, and a shear has no
    /// rotation-then-scale equivalent.
    /// </para>
    /// <para>
    /// A mirror comes out as a negative vertical scale, which can be drawn as a vertical flip. A horizontal
    /// mirror is reported the same way, combined with a half-turn rotation.
    /// </para>
    /// </remarks>
    /// <param name="translation">When this method returns <see langword="true"/>, the translation; otherwise, <see cref="Point.Origin"/>.</param>
    /// <param name="rotation">When this method returns <see langword="true"/>, the rotation, clockwise on screen; otherwise, <see cref="Angle.Zero"/>.</param>
    /// <param name="scale">When this method returns <see langword="true"/>, the scale; otherwise, <see cref="Vector2.One"/>.</param>
    /// <returns><see langword="true"/> if the transform has no shear; otherwise, <see langword="false"/>.</returns>
    public bool TryDecompose(out Point translation, out Angle rotation, out Vector2 scale)
    {
        Matrix3x2 m = Matrix;

        float scaleX = MathF.Sqrt((m.M11 * m.M11) + (m.M12 * m.M12));
        float scaleY = MathF.Sqrt((m.M21 * m.M21) + (m.M22 * m.M22));
        float shear = (m.M11 * m.M21) + (m.M12 * m.M22);

        if (MathF.Abs(shear) > ShearTolerance * scaleX * scaleY)
        {
            translation = Point.Origin;
            rotation = Angle.Zero;
            scale = Vector2.One;
            return false;
        }

        if (m.GetDeterminant() < 0f)
            scaleY = -scaleY;

        translation = (Point)m.Translation;
        rotation = Angle.FromRadians(MathF.Atan2(m.M12, m.M11));
        scale = new(scaleX, scaleY);

        return true;
    }

    /// <inheritdoc/>
    public bool Equals(Transform other) => Matrix.Equals(other.Matrix);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Transform other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Matrix.GetHashCode();

    /// <summary>
    /// Combines two transforms.
    /// </summary>
    /// <param name="left">The transform applied second.</param>
    /// <param name="right">The transform applied first.</param>
    /// <returns>A transform equivalent to applying <paramref name="right"/>, then <paramref name="left"/>.</returns>
    public static Transform operator *(Transform left, Transform right) => new(right.Matrix * left.Matrix);

    /// <summary>
    /// Determines whether two transforms are exactly equal.
    /// </summary>
    /// <remarks>
    /// Two transforms built by different sequences of operations can be equivalent and still differ by
    /// floating-point rounding.
    /// </remarks>
    /// <param name="left">The first transform.</param>
    /// <param name="right">The second transform.</param>
    /// <returns><see langword="true"/> if every matrix component is equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Transform left, Transform right) => left.Equals(right);

    /// <summary>
    /// Determines whether two transforms differ.
    /// </summary>
    /// <param name="left">The first transform.</param>
    /// <param name="right">The second transform.</param>
    /// <returns><see langword="true"/> if any matrix component differs; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Transform left, Transform right) => !left.Equals(right);
}
