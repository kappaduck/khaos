// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Numerics;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class Vector2ExtensionsTests
{
    private const float Tolerance = 1e-4f;

    [Test]
    public async Task UpShouldPointTowardTheTopOfTheScreen()
    {
        await Vector2.Up.Should().BeEqualTo(new Vector2(0f, -1f));
    }

    [Test]
    public async Task DownShouldPointTowardTheBottomOfTheScreen()
    {
        await Vector2.Down.Should().BeEqualTo(new Vector2(0f, 1f));
    }

    [Test]
    public async Task LeftShouldPointTowardNegativeX()
    {
        await Vector2.Left.Should().BeEqualTo(new Vector2(-1f, 0f));
    }

    [Test]
    public async Task RightShouldPointTowardPositiveX()
    {
        await Vector2.Right.Should().BeEqualTo(new Vector2(1f, 0f));
    }

    [Test]
    public async Task FromAngleShouldTurnClockwiseFromRight()
    {
        Vector2 aim = Vector2.FromAngle(Angle.FromDegrees(90f), 5f);

        await Vector2.Distance(aim, new Vector2(0f, 5f)).Should().BeLessThan(Tolerance);
    }

    [Test]
    public async Task DirectionOfDownShouldBeAQuarterTurnClockwise()
    {
        await Vector2.Down.Direction.Degrees.Should().BeCloseTo(90f, Tolerance);
    }

    [Test]
    public async Task DirectionOfUpShouldBeAQuarterTurnCounterClockwise()
    {
        await Vector2.Up.Direction.Degrees.Should().BeCloseTo(-90f, Tolerance);
    }

    [Test]
    public async Task AngleBetweenShouldBePositiveWhenTurningClockwise()
    {
        Angle turn = Vector2.AngleBetween(Vector2.Right, Vector2.Down);

        await turn.Degrees.Should().BeCloseTo(90f, Tolerance);
    }

    [Test]
    public async Task AngleBetweenShouldBeNegativeWhenTurningCounterClockwise()
    {
        Angle turn = Vector2.AngleBetween(Vector2.Right, Vector2.Up);

        await turn.Degrees.Should().BeCloseTo(-90f, Tolerance);
    }

    [Test]
    public async Task PerpendicularClockwiseShouldTurnAQuarterTurnClockwise()
    {
        await Vector2.Right.PerpendicularClockwise.Should().BeEqualTo(Vector2.Down);
    }

    [Test]
    public async Task PerpendicularCounterClockwiseShouldTurnAQuarterTurnCounterClockwise()
    {
        await Vector2.Right.PerpendicularCounterClockwise.Should().BeEqualTo(Vector2.Up);
    }

    [Test]
    public async Task RotateShouldAgreeWithATransformRotation()
    {
        Vector2 velocity = new(3f, 1f);
        Angle turn = Angle.FromDegrees(37f);

        Vector2 rotated = velocity.Rotate(turn);

        await Vector2.Distance(rotated, Transform.Rotation(turn).TransformVector(velocity)).Should().BeLessThan(Tolerance);
    }

    [Test]
    public async Task NormalizeOrZeroShouldGiveZeroForAZeroVector()
    {
        await Vector2.Zero.NormalizeOrZero().Should().BeEqualTo(Vector2.Zero);
    }

    [Test]
    public async Task NormalizeOrZeroShouldGiveAUnitVectorInTheSameDirection()
    {
        Vector2 direction = new Vector2(3f, 4f).NormalizeOrZero();

        await Vector2.Distance(direction, new Vector2(0.6f, 0.8f)).Should().BeLessThan(Tolerance);
    }

    [Test]
    public async Task ClampLengthShouldShortenALongVectorKeepingItsDirection()
    {
        Vector2 clamped = new Vector2(3f, 4f).ClampLength(2.5f);

        await Vector2.Distance(clamped, new Vector2(1.5f, 2f)).Should().BeLessThan(Tolerance);
    }

    [Test]
    public async Task ClampLengthShouldLeaveAShortVectorUnchanged()
    {
        await new Vector2(1f, 1f).ClampLength(10f).Should().BeEqualTo(new Vector2(1f, 1f));
    }

    [Test]
    public async Task ClampLengthShouldRejectANegativeLength()
    {
        await Assert.That(() => new Vector2(1f, 1f).ClampLength(-1f)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task MoveTowardsShouldStepByTheMaximumDistance()
    {
        Vector2 moved = new Vector2(0f, 0f).MoveTowards(new Vector2(10f, 0f), 3f);

        await moved.Should().BeEqualTo(new Vector2(3f, 0f));
    }

    [Test]
    public async Task MoveTowardsShouldStopOnTheTargetWithoutOvershooting()
    {
        Vector2 moved = new Vector2(9f, 0f).MoveTowards(new Vector2(10f, 0f), 3f);

        await moved.Should().BeEqualTo(new Vector2(10f, 0f));
    }

    [Test]
    public async Task MoveTowardsShouldStayOnTheTargetOnceThere()
    {
        Vector2 target = new(10f, 0f);

        await target.MoveTowards(target, 3f).Should().BeEqualTo(target);
    }
}
