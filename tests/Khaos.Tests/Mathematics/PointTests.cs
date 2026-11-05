// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace KappaDuck.Khaos.Tests.Mathematics;

internal sealed class PointTests
{
    [Test]
    public async Task SubtractingTwoPointsShouldGiveTheDisplacementBetweenThem()
    {
        Point player = new(100f, 50f);
        Point enemy = new(130f, 10f);

        Vector2 toEnemy = enemy - player;

        await toEnemy.Should().BeEqualTo(new Vector2(30f, -40f));
    }

    [Test]
    public async Task AddingADisplacementShouldMoveThePoint()
    {
        Point ball = new(320f, 240f);
        Vector2 velocity = new(120f, -60f);

        Point next = ball + (velocity * 0.5f);

        await next.Should().BeEqualTo(new Point(380f, 210f));
    }

    [Test]
    public async Task SubtractingADisplacementShouldMoveThePointBack()
    {
        Point result = new Point(10f, 10f) - new Vector2(4f, 6f);

        await result.Should().BeEqualTo(new Point(6f, 4f));
    }

    [Test]
    public async Task DistanceShouldMeasureBetweenTwoPoints()
    {
        float distance = Point.Distance(new Point(1f, 2f), new Point(4f, 6f));

        await distance.Should().BeEqualTo(5f);
    }

    [Test]
    public async Task DistanceSquaredShouldBeTheSquareOfTheDistance()
    {
        float distance = Point.DistanceSquared(new Point(1f, 2f), new Point(4f, 6f));

        await distance.Should().BeEqualTo(25f);
    }

    [Test]
    public async Task LerpShouldInterpolateBetweenTwoPoints()
    {
        Point result = Point.Lerp(new Point(0f, 0f), new Point(10f, 20f), 0.5f);

        await result.Should().BeEqualTo(new Point(5f, 10f));
    }

    [Test]
    public async Task LerpShouldNotClampTheAmount()
    {
        Point result = Point.Lerp(new Point(0f, 0f), new Point(10f, 20f), 2f);

        await result.Should().BeEqualTo(new Point(20f, 40f));
    }

    [Test]
    [Arguments(0.5f, 1)]
    [Arguments(1.5f, 2)]
    [Arguments(2.5f, 3)]
    [Arguments(-0.5f, -1)]
    [Arguments(7.4f, 7)]
    public async Task RoundShouldSendHalvesAwayFromZero(float value, int expected)
    {
        PointI result = new Point(value, value).Round();

        await result.Should().BeEqualTo(new PointI(expected, expected));
    }

    [Test]
    public async Task FloorShouldRoundEachCoordinateDown()
    {
        PointI result = new Point(7.6f, -7.6f).Floor();

        await result.Should().BeEqualTo(new PointI(7, -8));
    }

    [Test]
    public async Task CeilingShouldRoundEachCoordinateUp()
    {
        PointI result = new Point(7.6f, -7.6f).Ceiling();

        await result.Should().BeEqualTo(new PointI(8, -7));
    }

    [Test]
    public async Task TruncateShouldRoundEachCoordinateTowardZero()
    {
        PointI result = new Point(7.6f, -7.6f).Truncate();

        await result.Should().BeEqualTo(new PointI(7, -7));
    }

    [Test]
    public async Task EqualsShouldReturnTrueForAPointWithTheSameCoordinates()
    {
        Point point = new(3.25f, -1f);

        bool result = point.Equals(new Point(3.25f, -1f));

        await result.Should().BeTrue();
    }

    [Test]
    [Arguments(4.25f, -1f)]
    [Arguments(3.25f, 0f)]
    public async Task EqualsShouldReturnFalseForAPointThatDiffersByOneCoordinate(float x, float y)
    {
        Point point = new(3.25f, -1f);

        bool result = point.Equals(new Point(x, y));

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsShouldTreatNaNAsEqualToNaN()
    {
        Point point = new(float.NaN, 0f);

        bool result = point.Equals(new Point(float.NaN, 0f));

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsShouldReturnTrueForNegativeZero()
    {
        Point point = new(0f, 0f);

        bool result = point.Equals(new Point(float.NegativeZero, 0f));

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnTrueForTheSamePointBoxed()
    {
        Point point = new(3.25f, -1f);
        object boxed = new Point(3.25f, -1f);

        bool result = point.Equals(boxed);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForNull()
    {
        Point point = new(3.25f, -1f);

        bool result = point.Equals(null);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForAnotherType()
    {
        Point point = new(3.25f, -1f);
        object vector = new Vector2(3.25f, -1f);

        bool result = point.Equals(vector);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task GetHashCodeShouldMatchForEqualPoints()
    {
        int hash = new Point(3.25f, -1f).GetHashCode();

        await hash.Should().BeEqualTo(new Point(3.25f, -1f).GetHashCode());
    }

    [Test]
    public async Task GetHashCodeShouldMatchForZeroAndNegativeZero()
    {
        int hash = new Point(0f, 0f).GetHashCode();

        await hash.Should().BeEqualTo(new Point(float.NegativeZero, 0f).GetHashCode());
    }

    [Test]
    public async Task EqualityShouldReturnTrueForIdenticalPoints()
    {
        Point point = new(1f, 1f);

        bool result = point == new Point(1f, 1f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldReturnFalseForPointsThatDifferByTheSmallestAmount()
    {
        bool result = new Point(1f, 1f) == new Point(1f + 1e-6f, 1f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualityShouldTreatNaNAsEqualToNaN()
    {
        Point point = new(float.NaN, 0f);

        bool result = point == new Point(float.NaN, 0f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task InequalityShouldReturnTrueForDifferentPoints()
    {
        bool result = new Point(1f, 1f) != new Point(1f, 2f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task InequalityShouldReturnFalseForIdenticalPoints()
    {
        Point point = new(1f, 1f);

        bool result = point != new Point(1f, 1f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task IsApproximatelyShouldReturnTrueForAPointWithinTheTolerance()
    {
        bool result = new Point(100.05f, 99.95f).IsApproximately(new Point(100f, 100f), 0.1f);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task IsApproximatelyShouldReturnFalseForAPointOutsideTheTolerance()
    {
        bool result = new Point(100.2f, 100f).IsApproximately(new Point(100f, 100f), 0.1f);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task ConvertingToVector2ShouldKeepTheCoordinates()
    {
        Vector2 vector = (Vector2)new Point(3f, -4f);

        await vector.Should().BeEqualTo(new Vector2(3f, -4f));
    }

    [Test]
    public async Task ConvertingFromVector2ShouldKeepTheCoordinates()
    {
        Point point = (Point)new Vector2(3f, -4f);

        await point.Should().BeEqualTo(new Point(3f, -4f));
    }

    [Test]
    public async Task WithShouldReplaceASingleCoordinate()
    {
        Point grounded = new Point(42f, 17.5f) with { Y = 0f };

        await grounded.Should().BeEqualTo(new Point(42f, 0f));
    }

    [Test]
    public async Task DeconstructShouldGiveBothCoordinates()
    {
        (float x, float y) = new Point(3f, 4f);

        await (x, y).Should().BeEqualTo((3f, 4f));
    }

    [Test]
    public async Task ToStringShouldUseTheCurrentCulture()
    {
        Point value = new(1.5f, 2.5f);

        string result = value.ToString();

        await result.Should().BeEqualTo(value.ToString(null, CultureInfo.CurrentCulture));
    }

    [Test]
    public async Task ToStringShouldSeparateWithCommasForTheInvariantCulture()
    {
        string result = new Point(1.5f, 2.5f).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(1.5, 2.5)");
    }

    [Test]
    public async Task ToStringShouldSeparateWithSemicolonsWhenTheDecimalSeparatorIsAComma()
    {
        string result = new Point(1.5f, 2.5f).ToString(null, CultureInfo.GetCultureInfo("fr-CA"));

        await result.Should().BeEqualTo("(1,5; 2,5)");
    }

    [Test]
    public async Task ToStringShouldApplyTheFormatToEachCoordinate()
    {
        string result = string.Create(CultureInfo.InvariantCulture, $"{new Point(1.5f, 2f):F1}");

        await result.Should().BeEqualTo("(1.5, 2.0)");
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteTheSameTextAsToString()
    {
        byte[] buffer = new byte[32];

        new Point(1.5f, 2.5f).TryFormat(buffer, out int written, default, CultureInfo.GetCultureInfo("fr-CA"));

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo("(1,5; 2,5)");
    }

    [Test]
    public async Task TryFormatShouldReturnFalseWhenTheDestinationIsTooSmall()
    {
        char[] buffer = new char[5];

        bool result = new Point(1.5f, 2.5f).TryFormat(buffer, out _, default, CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task TryFormatShouldWriteNothingWhenTheDestinationIsTooSmall()
    {
        char[] buffer = new char[5];

        new Point(1.5f, 2.5f).TryFormat(buffer, out int written, default, CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(0);
    }
}
