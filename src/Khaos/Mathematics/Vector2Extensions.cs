// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Numerics;

namespace KappaDuck.Khaos.Mathematics;

/// <summary>
/// Adds to <see cref="Vector2"/> what a 2D game needs and <see cref="System.Numerics"/> does not provide.
/// </summary>
/// <remarks>
/// Directions follow the screen: the Y axis points down, so <c>Vector2.Up</c> is (0, -1) and positive
/// angles turn clockwise.
/// </remarks>
public static class Vector2Extensions
{
    extension(Vector2)
    {
        /// <summary>
        /// Gets the unit vector pointing up on screen, (0, -1).
        /// </summary>
        public static Vector2 Up => new(0f, -1f);

        /// <summary>
        /// Gets the unit vector pointing down on screen, (0, 1).
        /// </summary>
        public static Vector2 Down => new(0f, 1f);

        /// <summary>
        /// Gets the unit vector pointing left, (-1, 0).
        /// </summary>
        public static Vector2 Left => new(-1f, 0f);

        /// <summary>
        /// Gets the unit vector pointing right, (1, 0).
        /// </summary>
        public static Vector2 Right => new(1f, 0f);

        /// <summary>
        /// Creates the vector of a given length pointing in a direction.
        /// </summary>
        /// <param name="direction">The direction, clockwise on screen from <c>Vector2.Right</c>.</param>
        /// <param name="length">The length of the vector.</param>
        /// <returns>The vector.</returns>
        public static Vector2 FromAngle(Angle direction, float length = 1f)
        {
            (float sin, float cos) = MathF.SinCos(direction.Radians);
            return new(cos * length, sin * length);
        }

        /// <summary>
        /// Computes the signed angle that turns one vector onto the direction of another.
        /// </summary>
        /// <param name="from">The starting direction.</param>
        /// <param name="to">The target direction.</param>
        /// <returns>The angle in [-180°, 180°], positive when turning clockwise on screen.</returns>
        public static Angle AngleBetween(Vector2 from, Vector2 to)
            => Angle.FromRadians(MathF.Atan2(Vector2.Cross(from, to), Vector2.Dot(from, to)));
    }

    extension(Vector2 vector)
    {
        /// <summary>
        /// Gets the direction of the vector, clockwise on screen from <c>Vector2.Right</c>.
        /// </summary>
        public Angle Direction => Angle.FromRadians(MathF.Atan2(vector.Y, vector.X));

        /// <summary>
        /// Gets the vector turned a quarter turn clockwise on screen.
        /// </summary>
        public Vector2 PerpendicularClockwise => new(-vector.Y, vector.X);

        /// <summary>
        /// Gets the vector turned a quarter turn counter-clockwise on screen.
        /// </summary>
        public Vector2 PerpendicularCounterClockwise => new(vector.Y, -vector.X);

        /// <summary>
        /// Returns the vector turned by an angle.
        /// </summary>
        /// <param name="angle">The angle, clockwise on screen.</param>
        /// <returns>The rotated vector.</returns>
        public Vector2 Rotate(Angle angle)
        {
            (float sin, float cos) = MathF.SinCos(angle.Radians);
            return new((vector.X * cos) - (vector.Y * sin), (vector.X * sin) + (vector.Y * cos));
        }

        /// <summary>
        /// Returns the vector with a length of one, or <see cref="Vector2.Zero"/> if it has no length.
        /// </summary>
        /// <remarks>
        /// <see cref="Vector2.Normalize(Vector2)"/> returns NaN for a zero vector, which then spreads through every
        /// position it touches. Use this one for input directions, which are zero whenever nothing is pressed.
        /// </remarks>
        /// <returns>The normalized vector, or <see cref="Vector2.Zero"/>.</returns>
        public Vector2 NormalizeOrZero()
        {
            float length = vector.Length();
            return length > 0f ? vector / length : Vector2.Zero;
        }

        /// <summary>
        /// Returns the vector shortened to a maximum length, keeping its direction.
        /// </summary>
        /// <remarks>
        /// Not <see cref="Vector2.Clamp(Vector2, Vector2, Vector2)"/>, which clamps each component separately.
        /// </remarks>
        /// <param name="maxLength">The maximum length.</param>
        /// <returns>The vector, shortened if it was longer than <paramref name="maxLength"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
        public Vector2 ClampLength(float maxLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

            float lengthSquared = vector.LengthSquared();

            if (lengthSquared <= maxLength * maxLength)
                return vector;

            return vector * (maxLength / MathF.Sqrt(lengthSquared));
        }

        /// <summary>
        /// Returns the vector moved toward a target by at most a given distance, without overshooting it.
        /// </summary>
        /// <param name="target">The vector to move toward.</param>
        /// <param name="maxDistance">The largest distance to move.</param>
        /// <returns><paramref name="target"/> if it is within <paramref name="maxDistance"/>; otherwise, the moved vector.</returns>
        public Vector2 MoveTowards(Vector2 target, float maxDistance)
        {
            Vector2 delta = target - vector;
            float distance = delta.Length();

            if (distance <= maxDistance || distance == 0f)
                return target;

            return vector + (delta * (maxDistance / distance));
        }
    }
}
