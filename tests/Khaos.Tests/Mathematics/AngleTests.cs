// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Text;

namespace KappaDuck.Khaos.Tests.Mathematics;

public sealed class AngleTests
{
    private const float Tolerance = 1e-4f;

    [Test]
    public async Task FromDegreesShouldStoreTheAngleInRadians()
    {
        Angle angle = Angle.FromDegrees(180f);

        await angle.Radians.Should().BeCloseTo(MathF.PI, Tolerance);
    }

    [Test]
    public async Task FromRadiansShouldKeepTheValue()
    {
        Angle angle = Angle.FromRadians(MathF.PI / 2f);

        await angle.Radians.Should().BeEqualTo(MathF.PI / 2f);
    }

    [Test]
    public async Task DegreesShouldConvertFromRadians()
    {
        Angle angle = Angle.FromRadians(MathF.PI / 2f);

        await angle.Degrees.Should().BeCloseTo(90f, Tolerance);
    }

    [Test]
    public async Task SinShouldBeTheSineOfTheAngle()
    {
        Angle angle = Angle.FromDegrees(30f);

        await angle.Sin.Should().BeCloseTo(0.5f, Tolerance);
    }

    [Test]
    public async Task CosShouldBeTheCosineOfTheAngle()
    {
        Angle angle = Angle.FromDegrees(60f);

        await angle.Cos.Should().BeCloseTo(0.5f, Tolerance);
    }

    [Test]
    public async Task TanShouldBeTheTangentOfTheAngle()
    {
        Angle angle = Angle.FromDegrees(45f);

        await angle.Tan.Should().BeCloseTo(1f, Tolerance);
    }

    [Test]
    [Arguments(450f, 90f)]
    [Arguments(-90f, 270f)]
    [Arguments(360f, 0f)]
    [Arguments(0f, 0f)]
    [Arguments(-720f, 0f)]
    public async Task NormalizeShouldWrapIntoZeroToFullTurn(float degrees, float expected)
    {
        Angle normalized = Angle.FromDegrees(degrees).Normalize();

        await normalized.Degrees.Should().BeCloseTo(expected, Tolerance);
    }

    [Test]
    public async Task NormalizeShouldNotReturnAFullTurnForATinyNegativeAngle()
    {
        Angle normalized = Angle.FromRadians(-1e-9f).Normalize();

        await normalized.Radians.Should().BeLessThan(float.Tau);
    }

    [Test]
    [Arguments(270f, -90f)]
    [Arguments(180f, -180f)]
    [Arguments(-180f, -180f)]
    [Arguments(90f, 90f)]
    [Arguments(-190f, 170f)]
    public async Task NormalizeSignedShouldWrapIntoHalfTurns(float degrees, float expected)
    {
        Angle normalized = Angle.FromDegrees(degrees).NormalizeSigned();

        await normalized.Degrees.Should().BeCloseTo(expected, Tolerance);
    }

    [Test]
    public async Task IsApproximatelyShouldWrapAroundAFullTurn()
    {
        bool result = Angle.FromDegrees(359.9f).IsApproximately(Angle.FromDegrees(0.1f), Angle.FromDegrees(0.5f));

        await result.Should().BeTrue();
    }

    [Test]
    public async Task IsApproximatelyShouldRejectAnglesFurtherThanTheTolerance()
    {
        bool result = Angle.FromDegrees(10f).IsApproximately(Angle.FromDegrees(12f), Angle.FromDegrees(1f));

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualityShouldAcceptTheSameAngle()
    {
        Angle angle = Angle.FromRadians(1f);

        bool result = angle == Angle.FromRadians(1f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldRejectAnglesThatDifferByTheSmallestAmount()
    {
        bool result = Angle.FromRadians(1f) == Angle.FromRadians(1f + 1e-6f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task InequalityShouldHoldForDifferentAngles()
    {
        bool result = Angle.FromRadians(1f) != Angle.FromRadians(2f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task LessThanShouldHoldForAnglesThatDifferByTheSmallestAmount()
    {
        bool result = Angle.FromRadians(1f) < Angle.FromRadians(1f + 1e-6f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task CompareToShouldBeNegativeForASmallerAngle()
    {
        int result = Angle.FromRadians(1f).CompareTo(Angle.FromRadians(2f));

        await result.Should().BeLessThan(0);
    }

    [Test]
    public async Task EqualAnglesShouldBeFoundInAHashSet()
    {
        HashSet<Angle> angles = [Angle.FromDegrees(45f)];

        await angles.Contains(Angle.FromDegrees(45f)).Should().BeTrue();
    }

    [Test]
    public async Task AdditionShouldSumTheAngles()
    {
        Angle result = Angle.FromDegrees(30f) + Angle.FromDegrees(45f);

        await result.Degrees.Should().BeCloseTo(75f, Tolerance);
    }

    [Test]
    public async Task SubtractionShouldGiveTheDifference()
    {
        Angle result = Angle.FromDegrees(45f) - Angle.FromDegrees(30f);

        await result.Degrees.Should().BeCloseTo(15f, Tolerance);
    }

    [Test]
    public async Task NegationShouldTurnTheOtherWay()
    {
        Angle result = -Angle.FromDegrees(30f);

        await result.Degrees.Should().BeCloseTo(-30f, Tolerance);
    }

    [Test]
    public async Task MultiplyingByAScalarShouldScaleTheAngle()
    {
        Angle result = Angle.FromDegrees(30f) * 2f;

        await result.Degrees.Should().BeCloseTo(60f, Tolerance);
    }

    [Test]
    public async Task MultiplyingAScalarByAnAngleShouldScaleTheAngle()
    {
        Angle result = 3f * Angle.FromDegrees(30f);

        await result.Degrees.Should().BeCloseTo(90f, Tolerance);
    }

    [Test]
    public async Task DivisionShouldDivideTheAngle()
    {
        Angle result = Angle.FromDegrees(45f) / 3f;

        await result.Degrees.Should().BeCloseTo(15f, Tolerance);
    }

    [Test]
    public async Task DividingByZeroShouldGiveAnInfiniteAngle()
    {
        Angle result = Angle.FromDegrees(90f) / 0f;

        await float.IsPositiveInfinity(result.Radians).Should().BeTrue();
    }

    [Test]
    public async Task ToStringShouldWriteDegrees()
    {
        string result = Angle.FromDegrees(90f).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("90°");
    }

    [Test]
    public async Task ToStringShouldApplyTheFormatAndTheProvider()
    {
        string result = Angle.FromDegrees(90f).ToString("F1", CultureInfo.GetCultureInfo("fr-CA"));

        await result.Should().BeEqualTo("90,0°");
    }

    [Test]
    public async Task InterpolationShouldApplyTheFormat()
    {
        string result = string.Create(CultureInfo.InvariantCulture, $"{Angle.FromDegrees(90f):F1}");

        await result.Should().BeEqualTo("90.0°");
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteDegrees()
    {
        byte[] buffer = new byte[16];

        Angle.FromDegrees(90f).TryFormat(buffer, out int written, "F1", CultureInfo.InvariantCulture);

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo("90.0°");
    }

    [Test]
    public async Task TryFormatShouldFailWhenTheDestinationIsTooSmall()
    {
        char[] buffer = new char[2];

        bool result = Angle.FromDegrees(90f).TryFormat(buffer, out _, default, CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task TryFormatShouldWriteNothingWhenTheDestinationIsTooSmall()
    {
        char[] buffer = new char[2];

        Angle.FromDegrees(90f).TryFormat(buffer, out int written, default, CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(0);
    }
}
