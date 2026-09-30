// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Numerics;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class TransformTests
{
    private const float Tolerance = 1e-4f;

    [Test]
    public async Task IdentityShouldLeaveAPointUnchanged()
    {
        Point point = Transform.Identity.TransformPoint(new Point(3f, -7f));

        await point.Should().BeEqualTo(new Point(3f, -7f));
    }

    [Test]
    public async Task DefaultShouldCollapseEveryPointOntoTheOrigin()
    {
        Transform unset = default;

        await unset.TransformPoint(new Point(3f, -7f)).Should().BeEqualTo(Point.Origin);
    }

    [Test]
    public async Task DefaultShouldNotBeTheIdentity()
    {
        Transform unset = default;

        await (unset == Transform.Identity).Should().BeFalse();
    }

    [Test]
    public async Task TranslationShouldMoveAPoint()
    {
        Point point = Transform.Translation(new Vector2(10f, 20f)).TransformPoint(new Point(1f, 2f));

        await point.Should().BeEqualTo(new Point(11f, 22f));
    }

    [Test]
    public async Task ScalingShouldMultiplyEachAxisByItsFactor()
    {
        Point point = Transform.Scaling(new Vector2(2f, 3f)).TransformPoint(new Point(4f, 5f));

        await point.Should().BeEqualTo(new Point(8f, 15f));
    }

    [Test]
    public async Task UniformScalingShouldMultiplyBothAxesByTheSameFactor()
    {
        Point point = Transform.Scaling(2f).TransformPoint(new Point(4f, 5f));

        await point.Should().BeEqualTo(new Point(8f, 10f));
    }

    [Test]
    [Arguments(0f, 1f, 0f)]
    [Arguments(90f, 0f, 1f)]
    [Arguments(180f, -1f, 0f)]
    [Arguments(270f, 0f, -1f)]
    public async Task RotationShouldTurnClockwiseOnScreen(float degrees, float expectedX, float expectedY)
    {
        Point point = Transform.Rotation(Angle.FromDegrees(degrees)).TransformPoint(new Point(1f, 0f));

        await point.IsApproximately(new Point(expectedX, expectedY), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task RotationAroundACenterShouldKeepTheCenterInPlace()
    {
        Point center = new(2f, 2f);

        Point point = Transform.Rotation(Angle.FromDegrees(90f), center).TransformPoint(center);

        await point.IsApproximately(center, Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task RotationAroundACenterShouldSwingAPointAroundIt()
    {
        Point point = Transform.Rotation(Angle.FromDegrees(90f), new Point(2f, 2f)).TransformPoint(new Point(3f, 2f));

        await point.IsApproximately(new Point(2f, 3f), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task MultiplicationShouldApplyTheRightOperandFirst()
    {
        Transform rotateAfterMoving = Transform.Rotation(Angle.FromDegrees(90f)) * Transform.Translation(new Vector2(100f, 0f));

        Point point = rotateAfterMoving.TransformPoint(new Point(1f, 0f));

        await point.IsApproximately(new Point(0f, 101f), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task SwappingTheOperandsShouldChangeTheResult()
    {
        Transform moveAfterRotating = Transform.Translation(new Vector2(100f, 0f)) * Transform.Rotation(Angle.FromDegrees(90f));

        Point point = moveAfterRotating.TransformPoint(new Point(1f, 0f));

        await point.IsApproximately(new Point(100f, 1f), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task CreateShouldPlaceTheOriginExactlyAtThePosition()
    {
        Transform sprite = Transform.Create(new Point(100f, 50f), Angle.FromDegrees(30f), new Vector2(2f, 2f), new Point(8f, 8f));
        Point pivot = sprite.TransformPoint(new Point(8f, 8f));

        await pivot.IsApproximately(new Point(100f, 50f), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task CreateShouldScaleThenMoveWithoutRotation()
    {
        Transform sprite = Transform.Create(new Point(10f, 10f), Angle.Zero, new Vector2(3f, 4f));

        await sprite.TransformPoint(new Point(2f, 1f)).Should().BeEqualTo(new Point(16f, 14f));
    }

    [Test]
    public async Task TransformVectorShouldIgnoreTheTranslation()
    {
        Transform transform = Transform.Create(new Point(500f, 500f), Angle.FromDegrees(90f), new Vector2(2f, 2f));

        Vector2 velocity = transform.TransformVector(new Vector2(1f, 0f));

        await Vector2.Distance(velocity, new Vector2(0f, 2f)).Should().BeLessThan(Tolerance);
    }

    [Test]
    public async Task TransformRectShouldGiveTheBoundsOfTheRotatedCorners()
    {
        Rect bounds = Transform.Rotation(Angle.FromDegrees(45f)).TransformRect(new Rect(0f, 0f, 10f, 10f));
        float diagonal = 10f * MathF.Sqrt(2f);

        await bounds.Size.IsApproximately(new Size(diagonal, diagonal), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task TryInvertShouldSucceedForAnInvertibleTransform()
    {
        Transform sprite = Transform.Create(new Point(120f, -40f), Angle.FromDegrees(33f), new Vector2(2f, 3f));

        bool inverted = sprite.TryInvert(out _);

        await inverted.Should().BeTrue();
    }

    [Test]
    public async Task TryInvertShouldUndoTheTransform()
    {
        Transform sprite = Transform.Create(new Point(120f, -40f), Angle.FromDegrees(33f), new Vector2(2f, 3f));
        Point start = new(7f, 9f);

        sprite.TryInvert(out Transform inverse);
        Point back = inverse.TransformPoint(sprite.TransformPoint(start));

        await back.IsApproximately(start, 1e-3f).Should().BeTrue();
    }

    [Test]
    public async Task TryInvertShouldFailWhenAnAxisIsFlattened()
    {
        bool inverted = Transform.Scaling(new Vector2(0f, 1f)).TryInvert(out _);

        await inverted.Should().BeFalse();
    }

    [Test]
    public async Task TryInvertShouldGiveTheIdentityWhenItFails()
    {
        Transform.Scaling(new Vector2(0f, 1f)).TryInvert(out Transform inverse);

        await inverse.Should().BeEqualTo(Transform.Identity);
    }

    [Test]
    public async Task TryDecomposeShouldSucceedForATransformFromCreate()
    {
        Transform sprite = Transform.Create(new Point(120f, -40f), Angle.FromDegrees(33f), new Vector2(2f, 3f));

        bool decomposed = sprite.TryDecompose(out _, out _, out _);

        await decomposed.Should().BeTrue();
    }

    [Test]
    public async Task TryDecomposeShouldRecoverTheTranslation()
    {
        Transform sprite = Transform.Create(new Point(120f, -40f), Angle.FromDegrees(33f), new Vector2(2f, 3f));

        sprite.TryDecompose(out Point translation, out _, out _);

        await translation.IsApproximately(new Point(120f, -40f), Tolerance).Should().BeTrue();
    }

    [Test]
    public async Task TryDecomposeShouldRecoverTheRotation()
    {
        Transform sprite = Transform.Create(new Point(120f, -40f), Angle.FromDegrees(33f), new Vector2(2f, 3f));

        sprite.TryDecompose(out _, out Angle rotation, out _);

        await rotation.Degrees.Should().BeCloseTo(33f, 1e-3f);
    }

    [Test]
    public async Task TryDecomposeShouldRecoverTheScale()
    {
        Transform sprite = Transform.Create(new Point(120f, -40f), Angle.FromDegrees(33f), new Vector2(2f, 3f));

        sprite.TryDecompose(out _, out _, out Vector2 scale);

        await Vector2.Distance(scale, new Vector2(2f, 3f)).Should().BeLessThan(Tolerance);
    }

    [Test]
    public async Task TryDecomposeShouldReportAMirrorAsANegativeVerticalScale()
    {
        Transform.Scaling(new Vector2(-2f, 3f)).TryDecompose(out _, out _, out Vector2 scale);

        await scale.Y.Should().BeLessThan(0f);
    }

    [Test]
    public async Task TryDecomposeShouldFailForANonUniformScaleAfterARotation()
    {
        Transform sheared = Transform.Scaling(new Vector2(2f, 1f)) * Transform.Rotation(Angle.FromDegrees(45f));

        bool decomposed = sheared.TryDecompose(out _, out _, out _);

        await decomposed.Should().BeFalse();
    }

    [Test]
    public async Task TryDecomposeShouldGiveNeutralValuesWhenItFails()
    {
        Transform sheared = Transform.Scaling(new Vector2(2f, 1f)) * Transform.Rotation(Angle.FromDegrees(45f));

        sheared.TryDecompose(out Point translation, out Angle rotation, out Vector2 scale);

        await (translation, rotation, scale).Should().BeEqualTo((Point.Origin, Angle.Zero, Vector2.One));
    }

    [Test]
    public async Task EqualityShouldAcceptTheSameTransform()
    {
        Transform translation = Transform.Translation(new Vector2(3f, 4f));

        bool result = translation == Transform.Translation(new Vector2(3f, 4f));

        await result.Should().BeTrue();
    }

    [Test]
    public async Task InequalityShouldHoldForDifferentTransforms()
    {
        bool result = Transform.Translation(new Vector2(3f, 4f)) != Transform.Translation(new Vector2(3f, 5f));

        await result.Should().BeTrue();
    }

    [Test]
    public async Task MatrixShouldRoundTripThroughTheConstructor()
    {
        Matrix3x2 matrix = Matrix3x2.CreateRotation(0.3f) * Matrix3x2.CreateTranslation(5f, 6f);

        await new Transform(matrix).Matrix.Should().BeEqualTo(matrix);
    }
}
