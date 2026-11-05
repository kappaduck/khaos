// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Text;

namespace KappaDuck.Khaos.Tests.Mathematics;

internal sealed class SizeITests
{
    [Test]
    [Arguments(10, 5, false)]
    [Arguments(0, 5, true)]
    [Arguments(5, 0, true)]
    [Arguments(-1, 5, true)]
    public async Task IsEmptyShouldReturnTrueForAZeroOrNegativeDimension(int width, int height, bool expected)
    {
        await new SizeI(width, height).IsEmpty.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task AreaShouldNotOverflowForLargeSizes()
    {
        SizeI world = new(50_000, 50_000);

        await world.Area.Should().BeEqualTo(2_500_000_000L);
    }

    [Test]
    public async Task MultiplyingByAWholeFactorShouldStayInteger()
    {
        SizeI result = new SizeI(640, 360) * 2;

        await result.Should().BeEqualTo(new SizeI(1280, 720));
    }

    [Test]
    public async Task MultiplyingAWholeFactorBySizeShouldStayInteger()
    {
        SizeI result = 3 * new SizeI(640, 360);

        await result.Should().BeEqualTo(new SizeI(1920, 1080));
    }

    [Test]
    public async Task MultiplyingByAFractionalFactorShouldGiveASize()
    {
        Size result = new SizeI(640, 360) * 1.5f;

        await result.Should().BeEqualTo(new Size(960f, 540f));
    }

    [Test]
    public async Task MultiplyingAFractionalFactorBySizeShouldGiveASize()
    {
        Size result = 1.5f * new SizeI(640, 360);

        await result.Should().BeEqualTo(new Size(960f, 540f));
    }

    [Test]
    public async Task ImplicitOperatorShouldConvertToSize()
    {
        Size size = new SizeI(1280, 720);

        await size.Should().BeEqualTo(new Size(1280f, 720f));
    }

    [Test]
    public async Task EqualsShouldReturnTrueForASizeWithTheSameDimensions()
    {
        SizeI size = new(1280, 720);

        bool result = size.Equals(new SizeI(1280, 720));

        await result.Should().BeTrue();
    }

    [Test]
    [Arguments(1281, 720)]
    [Arguments(1280, 721)]
    public async Task EqualsShouldReturnFalseForASizeThatDiffersByOneDimension(int width, int height)
    {
        SizeI size = new(1280, 720);

        bool result = size.Equals(new SizeI(width, height));

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnTrueForTheSameSizeBoxed()
    {
        SizeI size = new(1280, 720);
        object boxed = new SizeI(1280, 720);

        bool result = size.Equals(boxed);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForNull()
    {
        SizeI size = new(1280, 720);

        bool result = size.Equals(null);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForAnotherType()
    {
        SizeI size = new(1280, 720);
        object position = new PointI(1280, 720);

        bool result = size.Equals(position);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task GetHashCodeShouldMatchForEqualSizes()
    {
        int hash = new SizeI(1280, 720).GetHashCode();

        await hash.Should().BeEqualTo(new SizeI(1280, 720).GetHashCode());
    }

    [Test]
    public async Task EqualityShouldReturnTrueForIdenticalSizes()
    {
        SizeI size = new(1280, 720);

        bool result = size == new SizeI(1280, 720);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldReturnFalseForSwappedDimensions()
    {
        bool result = new SizeI(1280, 720) == new SizeI(720, 1280);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task InequalityShouldReturnTrueForDifferentSizes()
    {
        bool result = new SizeI(1280, 720) != new SizeI(1920, 1080);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task InequalityShouldReturnFalseForIdenticalSizes()
    {
        SizeI size = new(1280, 720);

        bool result = size != new SizeI(1280, 720);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task DeconstructShouldGiveBothDimensions()
    {
        (int width, int height) = new SizeI(1280, 720);

        await (width, height).Should().BeEqualTo((1280, 720));
    }

    [Test]
    public async Task ToStringShouldUseTheCurrentCulture()
    {
        SizeI value = new(1280, 720);

        string result = value.ToString();

        await result.Should().BeEqualTo(value.ToString(null, CultureInfo.CurrentCulture));
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteTheSameTextAsToString()
    {
        SizeI value = new(1280, 720);
        CultureInfo culture = CultureInfo.GetCultureInfo("fr-CA");
        byte[] buffer = new byte[64];

        value.TryFormat(buffer, out int written, default, culture);

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo(value.ToString(null, culture));
    }

    [Test]
    public async Task ToStringShouldWriteBothDimensions()
    {
        string result = new SizeI(1280, 720).ToString(null, CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(1280, 720)");
    }
}
