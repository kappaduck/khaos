// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Mathematics;
using System.Globalization;
using System.Text;

namespace KappaDuck.Khaos.Tests.Mathematics;

internal sealed class GeometryFormatterTests
{
    private const int FormattedLength = 10;

    [Test]
    [Arguments("", null, "(1.5, 2.5)")]
    [Arguments("en-US", null, "(1.5, 2.5)")]
    [Arguments("fr-CA", null, "(1,5; 2,5)")]
    [Arguments("de-DE", null, "(1,5; 2,5)")]
    [Arguments("", "F2", "(1.50, 2.50)")]
    [Arguments("fr-CA", "F2", "(1,50; 2,50)")]
    public async Task TryFormatShouldWriteTheExpectedText(string culture, string? format, string expected)
    {
        char[] buffer = new char[32];
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        GeometryFormatter.TryFormat(buffer, out int written, [1.5f, 2.5f], format, provider);

        await new string(buffer, 0, written).Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments("", null, "(1.5, 2.5)")]
    [Arguments("en-US", null, "(1.5, 2.5)")]
    [Arguments("fr-CA", null, "(1,5; 2,5)")]
    [Arguments("de-DE", null, "(1,5; 2,5)")]
    [Arguments("", "F2", "(1.50, 2.50)")]
    [Arguments("fr-CA", "F2", "(1,50; 2,50)")]
    public async Task TryFormatUtf8ShouldWriteTheExpectedText(string culture, string? format, string expected)
    {
        byte[] buffer = new byte[32];
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        GeometryFormatter.TryFormat(buffer, out int written, [1.5f, 2.5f], format, provider);

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments("", null, "(1.5, 2.5)")]
    [Arguments("en-US", null, "(1.5, 2.5)")]
    [Arguments("fr-CA", null, "(1,5; 2,5)")]
    [Arguments("de-DE", null, "(1,5; 2,5)")]
    [Arguments("", "F2", "(1.50, 2.50)")]
    [Arguments("fr-CA", "F2", "(1,50; 2,50)")]
    public async Task ToStringShouldWriteTheExpectedText(string culture, string? format, string expected)
    {
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        string result = GeometryFormatter.ToString(new Point(1.5f, 2.5f), format, provider);

        await result.Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments(1000f, 2000f, "", "N0", "(1,000; 2,000)")]
    [Arguments(1000f, 2000f, "", "#,##0", "(1,000; 2,000)")]
    [Arguments(1000f, 2000f, "en-US", "C0", "($1,000; $2,000)")]
    [Arguments(10f, 2000f, "", "N0", "(10; 2,000)")]
    [Arguments(10f, 20f, "", "N0", "(10, 20)")]
    [Arguments(1000f, 2000f, "de-DE", "N0", "(1.000; 2.000)")]
    public async Task TryFormatShouldWriteTheExpectedTextWhenDigitsAreGrouped(float x, float y, string culture, string format, string expected)
    {
        char[] buffer = new char[32];
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        GeometryFormatter.TryFormat(buffer, out int written, [x, y], format, provider);

        await new string(buffer, 0, written).Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments(1000f, 2000f, "", "N0", "(1,000; 2,000)")]
    [Arguments(1000f, 2000f, "", "#,##0", "(1,000; 2,000)")]
    [Arguments(1000f, 2000f, "en-US", "C0", "($1,000; $2,000)")]
    [Arguments(10f, 2000f, "", "N0", "(10; 2,000)")]
    [Arguments(10f, 20f, "", "N0", "(10, 20)")]
    [Arguments(1000f, 2000f, "de-DE", "N0", "(1.000; 2.000)")]
    public async Task TryFormatUtf8ShouldWriteTheExpectedTextWhenDigitsAreGrouped(float x, float y, string culture, string format, string expected)
    {
        byte[] buffer = new byte[32];
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        GeometryFormatter.TryFormat(buffer, out int written, [x, y], format, provider);

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo(expected);
    }

    [Test]
    public async Task TryFormatShouldUseSemicolonsForEverySeparatorWhenOneComponentContainsAComma()
    {
        char[] buffer = new char[32];

        GeometryFormatter.TryFormat(buffer, out int written, [10, 20, 3000, 40], "N0", CultureInfo.InvariantCulture);

        await new string(buffer, 0, written).Should().BeEqualTo("(10; 20; 3,000; 40)");
    }

    [Test]
    public async Task ToStringShouldSeparateWithSemicolonsWhenAComponentContainsAComma()
    {
        string result = GeometryFormatter.ToString(new Point(1000f, 2000f), "N0", CultureInfo.InvariantCulture);

        await result.Should().BeEqualTo("(1,000; 2,000)");
    }

    [Test]
    public async Task ToStringShouldUseTheCurrentCultureWhenTheProviderIsNull()
    {
        Point point = new(1.5f, 2.5f);

        string result = GeometryFormatter.ToString(point, null, null);

        await result.Should().BeEqualTo(GeometryFormatter.ToString(point, null, CultureInfo.CurrentCulture));
    }

    [Test]
    public async Task TryFormatShouldWriteEmptyParenthesesForNoComponents()
    {
        char[] buffer = new char[8];

        GeometryFormatter.TryFormat<float>(buffer, out int written, [], default, CultureInfo.InvariantCulture);

        await new string(buffer, 0, written).Should().BeEqualTo("()");
    }

    [Test]
    public async Task TryFormatShouldWriteASingleComponentWithoutSeparator()
    {
        char[] buffer = new char[8];

        GeometryFormatter.TryFormat(buffer, out int written, [3], default, CultureInfo.InvariantCulture);

        await new string(buffer, 0, written).Should().BeEqualTo("(3)");
    }

    [Test]
    public async Task TryFormatShouldSeparateEveryComponent()
    {
        char[] buffer = new char[32];

        GeometryFormatter.TryFormat(buffer, out int written, [-1, 2, 30, 40], default, CultureInfo.InvariantCulture);

        await new string(buffer, 0, written).Should().BeEqualTo("(-1, 2, 30, 40)");
    }

    [Test]
    public async Task TryFormatShouldReportTheNumberOfCharsWritten()
    {
        char[] buffer = new char[32];

        GeometryFormatter.TryFormat(buffer, out int written, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(FormattedLength);
    }

    [Test]
    public async Task TryFormatShouldReturnTrueWhenTheDestinationFitsExactly()
    {
        char[] buffer = new char[FormattedLength];

        bool result = GeometryFormatter.TryFormat(buffer, out _, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await result.Should().BeTrue();
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(5)]
    [Arguments(7)]
    [Arguments(9)]
    public async Task TryFormatShouldReturnFalseWhenTheDestinationIsTooSmall(int length)
    {
        char[] buffer = new char[length];

        bool result = GeometryFormatter.TryFormat(buffer, out _, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(5)]
    [Arguments(7)]
    [Arguments(9)]
    public async Task TryFormatShouldWriteNothingWhenTheDestinationIsTooSmall(int length)
    {
        char[] buffer = new char[length];

        GeometryFormatter.TryFormat(buffer, out int written, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(0);
    }

    [Test]
    public async Task TryFormatUtf8ShouldReturnTrueWhenTheDestinationFitsExactly()
    {
        byte[] buffer = new byte[FormattedLength];

        bool result = GeometryFormatter.TryFormat(buffer, out _, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await result.Should().BeTrue();
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(5)]
    [Arguments(7)]
    [Arguments(9)]
    public async Task TryFormatUtf8ShouldReturnFalseWhenTheDestinationIsTooSmall(int length)
    {
        byte[] buffer = new byte[length];

        bool result = GeometryFormatter.TryFormat(buffer, out _, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(5)]
    [Arguments(7)]
    [Arguments(9)]
    public async Task TryFormatUtf8ShouldWriteNothingWhenTheDestinationIsTooSmall(int length)
    {
        byte[] buffer = new byte[length];

        GeometryFormatter.TryFormat(buffer, out int written, [1.5f, 2.5f], default, CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(0);
    }
}
