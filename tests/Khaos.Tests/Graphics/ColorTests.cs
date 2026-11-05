// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using System.Globalization;
using System.Text;

namespace KappaDuck.Khaos.Tests.Graphics;

internal sealed class ColorTests
{
    [Test]
    public async Task ConstructorShouldStoreTheChannels()
    {
        Color color = new(18, 52, 86, 120);

        await (color.R, color.G, color.B, color.A).Should().BeEqualTo(((byte)18, (byte)52, (byte)86, (byte)120));
    }

    [Test]
    public async Task ConstructorShouldMakeTheColorOpaqueByDefault()
    {
        Color color = new(18, 52, 86);

        await color.A.Should().BeEqualTo(byte.MaxValue);
    }

    [Test]
    public async Task DefaultShouldBeTransparent()
    {
        Color color = default;

        await color.Should().BeEqualTo(Color.Transparent);
    }

    [Test]
    public async Task WithShouldReplaceASingleChannel()
    {
        Color faded = Color.CornflowerBlue with { A = 128 };

        await faded.Should().BeEqualTo(new Color(100, 149, 237, 128));
    }

    [Test]
    public async Task TransparentShouldHaveEveryChannelAtZero()
    {
        Color transparent = Color.Transparent;

        await transparent.Should().BeEqualTo(new Color(0, 0, 0, 0));
    }

    [Test]
    [MethodDataSource(nameof(NamedColors))]
    public async Task NamedColorShouldMatchItsCssValue(Color named, Color expected)
    {
        await named.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task DeconstructShouldGiveTheChannels()
    {
        (byte r, byte g, byte b, byte a) = new Color(18, 52, 86, 120);

        await (r, g, b, a).Should().BeEqualTo(((byte)18, (byte)52, (byte)86, (byte)120));
    }

    [Test]
    public async Task EqualsShouldReturnTrueForAColorWithTheSameChannels()
    {
        Color color = new(18, 52, 86, 120);

        bool result = color.Equals(new Color(18, 52, 86, 120));

        await result.Should().BeTrue();
    }

    [Test]
    [Arguments(19, 52, 86, 120)]
    [Arguments(18, 53, 86, 120)]
    [Arguments(18, 52, 87, 120)]
    [Arguments(18, 52, 86, 121)]
    public async Task EqualsShouldReturnFalseForAColorThatDiffersByOneChannel(int r, int g, int b, int a)
    {
        Color color = new(18, 52, 86, 120);
        Color other = new((byte)r, (byte)g, (byte)b, (byte)a);

        bool result = color.Equals(other);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnTrueForTheSameColorBoxed()
    {
        Color color = new(18, 52, 86, 120);
        object boxed = new Color(18, 52, 86, 120);

        bool result = color.Equals(boxed);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForNull()
    {
        Color color = new(18, 52, 86, 120);

        bool result = color.Equals(null);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForAnotherType()
    {
        Color color = new(18, 52, 86, 120);
        object packed = color.ToHex();

        bool result = color.Equals(packed);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task FromHexShouldReadAPackedRgbaValue()
    {
        Color color = Color.FromHex(0x6495EDFF);

        await color.Should().BeEqualTo(Color.CornflowerBlue);
    }

    [Test]
    public async Task FromHexShouldReadTheLowestByteAsAlpha()
    {
        Color color = Color.FromHex(0x6495ED);

        await color.Should().BeEqualTo(new Color(0x00, 0x64, 0x95, 0xED));
    }

    [Test]
    public async Task FromHexTextShouldReadHexadecimalText()
    {
        Color color = Color.FromHex("#6495ED");

        await color.Should().BeEqualTo(Color.CornflowerBlue);
    }

    [Test]
    public async Task FromHexTextShouldThrowForAnInvalidValue()
    {
        await Assert.That(static () => Color.FromHex("#nope")).Throws<FormatException>();
    }

    [Test]
    public async Task FromHexTextShouldNameTheInvalidValueInTheMessage()
    {
        FormatException exception = Assert.Throws<FormatException>(static () => Color.FromHex("#nope"));

        await exception.Message.Should().Contain("#nope");
    }

    [Test]
    public async Task GetHashCodeShouldMatchForEqualColors()
    {
        int hash = new Color(18, 52, 86, 120).GetHashCode();

        await hash.Should().BeEqualTo(new Color(18, 52, 86, 120).GetHashCode());
    }

    [Test]
    [Arguments(0f, 0, 0, 0, 255)]
    [Arguments(0.5f, 100, 50, 25, 128)]
    [Arguments(1f, 200, 100, 50, 0)]
    public async Task LerpShouldBlendBetweenTheTwoColors(float factor, int r, int g, int b, int a)
    {
        Color from = new(0, 0, 0, 255);
        Color to = new(200, 100, 50, 0);

        Color blended = Color.Lerp(from, to, factor);

        await blended.Should().BeEqualTo(new Color((byte)r, (byte)g, (byte)b, (byte)a));
    }

    [Test]
    [Arguments(-1f, 0f)]
    [Arguments(2f, 1f)]
    public async Task LerpShouldKeepTheFactorBetweenZeroAndOne(float factor, float kept)
    {
        Color from = new(0, 0, 0, 255);
        Color to = new(200, 100, 50, 0);

        Color blended = Color.Lerp(from, to, factor);

        await blended.Should().BeEqualTo(Color.Lerp(from, to, kept));
    }

    [Test]
    public async Task LerpShouldRoundHalvesAwayFromZero()
    {
        Color from = new(0, 0, 0, 0);
        Color to = new(1, 3, 5, 7);

        Color blended = Color.Lerp(from, to, 0.5f);

        await blended.Should().BeEqualTo(new Color(1, 2, 3, 4));
    }

    [Test]
    public async Task ToHexShouldPackTheChannelsAsRgba()
    {
        uint packed = new Color(0x12, 0x34, 0x56, 0x78).ToHex();

        await packed.Should().BeEqualTo(0x12345678u);
    }

    [Test]
    public async Task ToStringShouldListTheChannels()
    {
        string text = Color.CornflowerBlue.ToString(null, CultureInfo.InvariantCulture);

        await text.Should().BeEqualTo("(100, 149, 237, 255)");
    }

    [Test]
    public async Task ToStringShouldSeparateWithSemicolonsWhenTheDecimalSeparatorIsAComma()
    {
        string text = Color.CornflowerBlue.ToString(null, CultureInfo.GetCultureInfo("fr-CA"));

        await text.Should().BeEqualTo("(100; 149; 237; 255)");
    }

    [Test]
    public async Task ToStringShouldApplyTheFormatToEachChannel()
    {
        string text = new Color(1, 2, 3, 4).ToString("D3", CultureInfo.InvariantCulture);

        await text.Should().BeEqualTo("(001, 002, 003, 004)");
    }

    [Test]
    [Arguments("X", "#6495EDFF")]
    [Arguments("x", "#6495edff")]
    public async Task ToStringShouldWriteHexWithTheXFormat(string format, string expected)
    {
        string text = Color.CornflowerBlue.ToString(format, CultureInfo.InvariantCulture);

        await text.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task InterpolationShouldApplyTheXFormat()
    {
        string text = string.Create(CultureInfo.InvariantCulture, $"{Color.CornflowerBlue:X}");

        await text.Should().BeEqualTo("#6495EDFF");
    }

    [Test]
    public async Task TryFormatShouldWriteHexWithTheXFormat()
    {
        char[] buffer = new char[16];

        Color.CornflowerBlue.TryFormat(buffer, out int written, "X", CultureInfo.InvariantCulture);

        await new string(buffer, 0, written).Should().BeEqualTo("#6495EDFF");
    }

    [Test]
    public async Task TryFormatShouldReturnFalseWhenTheDestinationIsTooSmall()
    {
        char[] buffer = new char[8];

        bool result = Color.CornflowerBlue.TryFormat(buffer, out _, "X", CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task TryFormatShouldWriteNothingWhenTheDestinationIsTooSmall()
    {
        char[] buffer = new char[8];

        Color.CornflowerBlue.TryFormat(buffer, out int written, "X", CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(0);
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteLowercaseHexWithTheLowercaseXFormat()
    {
        byte[] buffer = new byte[16];

        Color.CornflowerBlue.TryFormat(buffer, out int written, "x", CultureInfo.InvariantCulture);

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo("#6495edff");
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteHexWithTheXFormat()
    {
        byte[] buffer = new byte[16];

        Color.CornflowerBlue.TryFormat(buffer, out int written, "X", CultureInfo.InvariantCulture);

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo("#6495EDFF");
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteTheSameTextAsToString()
    {
        byte[] buffer = new byte[32];

        Color.CornflowerBlue.TryFormat(buffer, out int written, default, CultureInfo.GetCultureInfo("fr-CA"));

        await Encoding.UTF8.GetString(buffer, 0, written).Should().BeEqualTo("(100; 149; 237; 255)");
    }

    [Test]
    public async Task TryFormatUtf8ShouldReturnFalseWhenTheDestinationIsTooSmall()
    {
        byte[] buffer = new byte[8];

        bool result = Color.CornflowerBlue.TryFormat(buffer, out _, "X", CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task TryFormatUtf8ShouldWriteNothingWhenTheDestinationIsTooSmall()
    {
        byte[] buffer = new byte[8];

        Color.CornflowerBlue.TryFormat(buffer, out int written, "X", CultureInfo.InvariantCulture);

        await written.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("#6495ED")]
    [Arguments("6495ED")]
    [Arguments("#6495ed80")]
    [Arguments("#abc")]
    [Arguments("#ABCD")]
    [Arguments(" #6495ED ")]
    public async Task TryFromHexShouldReturnTrueForEveryCssLayout(string hex)
    {
        bool parsed = Color.TryFromHex(hex, out _);

        await parsed.Should().BeTrue();
    }

    [Test]
    [Arguments("#6495ED", 100, 149, 237, 255)]
    [Arguments("6495ED", 100, 149, 237, 255)]
    [Arguments("#6495ed80", 100, 149, 237, 128)]
    [Arguments("#abc", 0xAA, 0xBB, 0xCC, 255)]
    [Arguments("#ABCD", 0xAA, 0xBB, 0xCC, 0xDD)]
    [Arguments(" #6495ED ", 100, 149, 237, 255)]
    public async Task TryFromHexShouldReadTheChannelsOfEveryCssLayout(string hex, int r, int g, int b, int a)
    {
        Color.TryFromHex(hex, out Color color);

        await color.Should().BeEqualTo(new Color((byte)r, (byte)g, (byte)b, (byte)a));
    }

    [Test]
    [Arguments("")]
    [Arguments("#")]
    [Arguments("#12")]
    [Arguments("#12345")]
    [Arguments("#1234567")]
    [Arguments("#123456789")]
    [Arguments("#GGGGGG")]
    [Arguments("##123456")]
    public async Task TryFromHexShouldReturnFalseForAnInvalidValue(string hex)
    {
        bool parsed = Color.TryFromHex(hex, out _);

        await parsed.Should().BeFalse();
    }

    [Test]
    [Arguments("")]
    [Arguments("#")]
    [Arguments("#12")]
    [Arguments("#12345")]
    [Arguments("#1234567")]
    [Arguments("#123456789")]
    [Arguments("#GGGGGG")]
    [Arguments("##123456")]
    public async Task TryFromHexShouldGiveTransparentForAnInvalidValue(string hex)
    {
        Color.TryFromHex(hex, out Color color);

        await color.Should().BeEqualTo(Color.Transparent);
    }

    [Test]
    public async Task EqualityShouldReturnTrueForAColorWithTheSameChannels()
    {
        Color color = new(18, 52, 86, 120);

        bool result = color == new Color(18, 52, 86, 120);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldReturnFalseForADifferentColor()
    {
        bool result = new Color(18, 52, 86, 120) == new Color(18, 52, 86, 121);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task InequalityShouldReturnTrueForDifferentColors()
    {
        bool result = new Color(18, 52, 86, 120) != new Color(18, 52, 86, 121);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task InequalityShouldReturnFalseForTheSameChannels()
    {
        Color color = new(18, 52, 86, 120);

        bool result = color != new Color(18, 52, 86, 120);

        await result.Should().BeFalse();
    }

    public static IEnumerable<Func<(Color Named, Color Expected)>> NamedColors()
    {
        yield return static () => (Color.Black, new Color(0, 0, 0));
        yield return static () => (Color.White, new Color(255, 255, 255));
        yield return static () => (Color.CornflowerBlue, new Color(100, 149, 237));
        yield return static () => (Color.RebeccaPurple, new Color(102, 51, 153));
        yield return static () => (Color.LightGoldenrodYellow, new Color(250, 250, 210));
    }
}
