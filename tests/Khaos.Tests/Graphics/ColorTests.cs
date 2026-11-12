// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using System.Globalization;
using System.Reflection;
using System.Text;
using TUnit.Assertions.Enums;
using TUnit.Core.Executors;

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
    public async Task NamedColorsShouldListEveryNamedColor()
    {
        IEnumerable<string> properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(static property => property.PropertyType == typeof(Color) && property.Name != nameof(Color.Transparent))
            .Select(static property => property.Name);

        IEnumerable<string?> tested = NamedColors().Select(static row => row.DisplayName);

        await properties.Should().BeEquivalentTo(tested, StringComparer.Ordinal, CollectionOrdering.Any);
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
    public async Task EqualsObjectShouldReturnFalseForADifferentColorBoxed()
    {
        Color color = new(18, 52, 86, 120);
        object boxed = new Color(18, 52, 86, 121);

        bool result = color.Equals(boxed);

        await result.Should().BeFalse();
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
    [Culture("fr-CA")]
    public async Task ToStringShouldUseTheCurrentCulture()
    {
        string text = Color.CornflowerBlue.ToString();

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
    [MethodDataSource(nameof(TooSmallHexDestinations))]
    public async Task TryFormatShouldReturnFalseWhenTheDestinationIsTooSmall(string format, int size)
    {
        char[] buffer = new char[size];

        bool result = Color.CornflowerBlue.TryFormat(buffer, out _, format, CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    [MethodDataSource(nameof(TooSmallHexDestinations))]
    public async Task TryFormatShouldWriteNothingWhenTheDestinationIsTooSmall(string format, int size)
    {
        char[] buffer = new char[size];

        Color.CornflowerBlue.TryFormat(buffer, out int written, format, CultureInfo.InvariantCulture);

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
    [MethodDataSource(nameof(TooSmallHexDestinations))]
    public async Task TryFormatUtf8ShouldReturnFalseWhenTheDestinationIsTooSmall(string format, int size)
    {
        byte[] buffer = new byte[size];

        bool result = Color.CornflowerBlue.TryFormat(buffer, out _, format, CultureInfo.InvariantCulture);

        await result.Should().BeFalse();
    }

    [Test]
    [MethodDataSource(nameof(TooSmallHexDestinations))]
    public async Task TryFormatUtf8ShouldWriteNothingWhenTheDestinationIsTooSmall(string format, int size)
    {
        byte[] buffer = new byte[size];

        Color.CornflowerBlue.TryFormat(buffer, out int written, format, CultureInfo.InvariantCulture);

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

    public static IEnumerable<TestDataRow<(Color Named, Color Expected)>> NamedColors()
    {
        yield return new((Color.AliceBlue, new Color(240, 248, 255)), nameof(Color.AliceBlue));
        yield return new((Color.AntiqueWhite, new Color(250, 235, 215)), nameof(Color.AntiqueWhite));
        yield return new((Color.Aqua, new Color(0, 255, 255)), nameof(Color.Aqua));
        yield return new((Color.Aquamarine, new Color(127, 255, 212)), nameof(Color.Aquamarine));
        yield return new((Color.Azure, new Color(240, 255, 255)), nameof(Color.Azure));
        yield return new((Color.Beige, new Color(245, 245, 220)), nameof(Color.Beige));
        yield return new((Color.Bisque, new Color(255, 228, 196)), nameof(Color.Bisque));
        yield return new((Color.Black, new Color(0, 0, 0)), nameof(Color.Black));
        yield return new((Color.BlanchedAlmond, new Color(255, 235, 205)), nameof(Color.BlanchedAlmond));
        yield return new((Color.Blue, new Color(0, 0, 255)), nameof(Color.Blue));
        yield return new((Color.BlueViolet, new Color(138, 43, 226)), nameof(Color.BlueViolet));
        yield return new((Color.Brown, new Color(165, 42, 42)), nameof(Color.Brown));
        yield return new((Color.BurlyWood, new Color(222, 184, 135)), nameof(Color.BurlyWood));
        yield return new((Color.CadetBlue, new Color(95, 158, 160)), nameof(Color.CadetBlue));
        yield return new((Color.Chartreuse, new Color(127, 255, 0)), nameof(Color.Chartreuse));
        yield return new((Color.Chocolate, new Color(210, 105, 30)), nameof(Color.Chocolate));
        yield return new((Color.Coral, new Color(255, 127, 80)), nameof(Color.Coral));
        yield return new((Color.CornflowerBlue, new Color(100, 149, 237)), nameof(Color.CornflowerBlue));
        yield return new((Color.Cornsilk, new Color(255, 248, 220)), nameof(Color.Cornsilk));
        yield return new((Color.Crimson, new Color(220, 20, 60)), nameof(Color.Crimson));
        yield return new((Color.Cyan, new Color(0, 255, 255)), nameof(Color.Cyan));
        yield return new((Color.DarkBlue, new Color(0, 0, 139)), nameof(Color.DarkBlue));
        yield return new((Color.DarkCyan, new Color(0, 139, 139)), nameof(Color.DarkCyan));
        yield return new((Color.DarkGoldenrod, new Color(184, 134, 11)), nameof(Color.DarkGoldenrod));
        yield return new((Color.DarkGray, new Color(169, 169, 169)), nameof(Color.DarkGray));
        yield return new((Color.DarkGreen, new Color(0, 100, 0)), nameof(Color.DarkGreen));
        yield return new((Color.DarkKhaki, new Color(189, 183, 107)), nameof(Color.DarkKhaki));
        yield return new((Color.DarkMagenta, new Color(139, 0, 139)), nameof(Color.DarkMagenta));
        yield return new((Color.DarkOliveGreen, new Color(85, 107, 47)), nameof(Color.DarkOliveGreen));
        yield return new((Color.DarkOrange, new Color(255, 140, 0)), nameof(Color.DarkOrange));
        yield return new((Color.DarkOrchid, new Color(153, 50, 204)), nameof(Color.DarkOrchid));
        yield return new((Color.DarkRed, new Color(139, 0, 0)), nameof(Color.DarkRed));
        yield return new((Color.DarkSalmon, new Color(233, 150, 122)), nameof(Color.DarkSalmon));
        yield return new((Color.DarkSeaGreen, new Color(143, 188, 143)), nameof(Color.DarkSeaGreen));
        yield return new((Color.DarkSlateBlue, new Color(72, 61, 139)), nameof(Color.DarkSlateBlue));
        yield return new((Color.DarkSlateGray, new Color(47, 79, 79)), nameof(Color.DarkSlateGray));
        yield return new((Color.DarkTurquoise, new Color(0, 206, 209)), nameof(Color.DarkTurquoise));
        yield return new((Color.DarkViolet, new Color(148, 0, 211)), nameof(Color.DarkViolet));
        yield return new((Color.DeepPink, new Color(255, 20, 147)), nameof(Color.DeepPink));
        yield return new((Color.DeepSkyBlue, new Color(0, 191, 255)), nameof(Color.DeepSkyBlue));
        yield return new((Color.DimGray, new Color(105, 105, 105)), nameof(Color.DimGray));
        yield return new((Color.DodgerBlue, new Color(30, 144, 255)), nameof(Color.DodgerBlue));
        yield return new((Color.Firebrick, new Color(178, 34, 34)), nameof(Color.Firebrick));
        yield return new((Color.FloralWhite, new Color(255, 250, 240)), nameof(Color.FloralWhite));
        yield return new((Color.ForestGreen, new Color(34, 139, 34)), nameof(Color.ForestGreen));
        yield return new((Color.Fuchsia, new Color(255, 0, 255)), nameof(Color.Fuchsia));
        yield return new((Color.Gainsboro, new Color(220, 220, 220)), nameof(Color.Gainsboro));
        yield return new((Color.GhostWhite, new Color(248, 248, 255)), nameof(Color.GhostWhite));
        yield return new((Color.Gold, new Color(255, 215, 0)), nameof(Color.Gold));
        yield return new((Color.Goldenrod, new Color(218, 165, 32)), nameof(Color.Goldenrod));
        yield return new((Color.Gray, new Color(128, 128, 128)), nameof(Color.Gray));
        yield return new((Color.Green, new Color(0, 128, 0)), nameof(Color.Green));
        yield return new((Color.GreenYellow, new Color(173, 255, 47)), nameof(Color.GreenYellow));
        yield return new((Color.Honeydew, new Color(240, 255, 240)), nameof(Color.Honeydew));
        yield return new((Color.HotPink, new Color(255, 105, 180)), nameof(Color.HotPink));
        yield return new((Color.IndianRed, new Color(205, 92, 92)), nameof(Color.IndianRed));
        yield return new((Color.Indigo, new Color(75, 0, 130)), nameof(Color.Indigo));
        yield return new((Color.Ivory, new Color(255, 255, 240)), nameof(Color.Ivory));
        yield return new((Color.Khaki, new Color(240, 230, 140)), nameof(Color.Khaki));
        yield return new((Color.Lavender, new Color(230, 230, 250)), nameof(Color.Lavender));
        yield return new((Color.LavenderBlush, new Color(255, 240, 245)), nameof(Color.LavenderBlush));
        yield return new((Color.LawnGreen, new Color(124, 252, 0)), nameof(Color.LawnGreen));
        yield return new((Color.LemonChiffon, new Color(255, 250, 205)), nameof(Color.LemonChiffon));
        yield return new((Color.LightBlue, new Color(173, 216, 230)), nameof(Color.LightBlue));
        yield return new((Color.LightCoral, new Color(240, 128, 128)), nameof(Color.LightCoral));
        yield return new((Color.LightCyan, new Color(224, 255, 255)), nameof(Color.LightCyan));
        yield return new((Color.LightGoldenrodYellow, new Color(250, 250, 210)), nameof(Color.LightGoldenrodYellow));
        yield return new((Color.LightGray, new Color(211, 211, 211)), nameof(Color.LightGray));
        yield return new((Color.LightGreen, new Color(144, 238, 144)), nameof(Color.LightGreen));
        yield return new((Color.LightPink, new Color(255, 182, 193)), nameof(Color.LightPink));
        yield return new((Color.LightSalmon, new Color(255, 160, 122)), nameof(Color.LightSalmon));
        yield return new((Color.LightSeaGreen, new Color(32, 178, 170)), nameof(Color.LightSeaGreen));
        yield return new((Color.LightSkyBlue, new Color(135, 206, 250)), nameof(Color.LightSkyBlue));
        yield return new((Color.LightSlateGray, new Color(119, 136, 153)), nameof(Color.LightSlateGray));
        yield return new((Color.LightSteelBlue, new Color(176, 196, 222)), nameof(Color.LightSteelBlue));
        yield return new((Color.LightYellow, new Color(255, 255, 224)), nameof(Color.LightYellow));
        yield return new((Color.Lime, new Color(0, 255, 0)), nameof(Color.Lime));
        yield return new((Color.LimeGreen, new Color(50, 205, 50)), nameof(Color.LimeGreen));
        yield return new((Color.Linen, new Color(250, 240, 230)), nameof(Color.Linen));
        yield return new((Color.Magenta, new Color(255, 0, 255)), nameof(Color.Magenta));
        yield return new((Color.Maroon, new Color(128, 0, 0)), nameof(Color.Maroon));
        yield return new((Color.MediumAquamarine, new Color(102, 205, 170)), nameof(Color.MediumAquamarine));
        yield return new((Color.MediumBlue, new Color(0, 0, 205)), nameof(Color.MediumBlue));
        yield return new((Color.MediumOrchid, new Color(186, 85, 211)), nameof(Color.MediumOrchid));
        yield return new((Color.MediumPurple, new Color(147, 112, 219)), nameof(Color.MediumPurple));
        yield return new((Color.MediumSeaGreen, new Color(60, 179, 113)), nameof(Color.MediumSeaGreen));
        yield return new((Color.MediumSlateBlue, new Color(123, 104, 238)), nameof(Color.MediumSlateBlue));
        yield return new((Color.MediumSpringGreen, new Color(0, 250, 154)), nameof(Color.MediumSpringGreen));
        yield return new((Color.MediumTurquoise, new Color(72, 209, 204)), nameof(Color.MediumTurquoise));
        yield return new((Color.MediumVioletRed, new Color(199, 21, 133)), nameof(Color.MediumVioletRed));
        yield return new((Color.MidnightBlue, new Color(25, 25, 112)), nameof(Color.MidnightBlue));
        yield return new((Color.MintCream, new Color(245, 255, 250)), nameof(Color.MintCream));
        yield return new((Color.MistyRose, new Color(255, 228, 225)), nameof(Color.MistyRose));
        yield return new((Color.Moccasin, new Color(255, 228, 181)), nameof(Color.Moccasin));
        yield return new((Color.NavajoWhite, new Color(255, 222, 173)), nameof(Color.NavajoWhite));
        yield return new((Color.Navy, new Color(0, 0, 128)), nameof(Color.Navy));
        yield return new((Color.OldLace, new Color(253, 245, 230)), nameof(Color.OldLace));
        yield return new((Color.Olive, new Color(128, 128, 0)), nameof(Color.Olive));
        yield return new((Color.OliveDrab, new Color(107, 142, 35)), nameof(Color.OliveDrab));
        yield return new((Color.Orange, new Color(255, 165, 0)), nameof(Color.Orange));
        yield return new((Color.OrangeRed, new Color(255, 69, 0)), nameof(Color.OrangeRed));
        yield return new((Color.Orchid, new Color(218, 112, 214)), nameof(Color.Orchid));
        yield return new((Color.PaleGoldenrod, new Color(238, 232, 170)), nameof(Color.PaleGoldenrod));
        yield return new((Color.PaleGreen, new Color(152, 251, 152)), nameof(Color.PaleGreen));
        yield return new((Color.PaleTurquoise, new Color(175, 238, 238)), nameof(Color.PaleTurquoise));
        yield return new((Color.PaleVioletRed, new Color(219, 112, 147)), nameof(Color.PaleVioletRed));
        yield return new((Color.PapayaWhip, new Color(255, 239, 213)), nameof(Color.PapayaWhip));
        yield return new((Color.PeachPuff, new Color(255, 218, 185)), nameof(Color.PeachPuff));
        yield return new((Color.Peru, new Color(205, 133, 63)), nameof(Color.Peru));
        yield return new((Color.Pink, new Color(255, 192, 203)), nameof(Color.Pink));
        yield return new((Color.Plum, new Color(221, 160, 221)), nameof(Color.Plum));
        yield return new((Color.PowderBlue, new Color(176, 224, 230)), nameof(Color.PowderBlue));
        yield return new((Color.Purple, new Color(128, 0, 128)), nameof(Color.Purple));
        yield return new((Color.RebeccaPurple, new Color(102, 51, 153)), nameof(Color.RebeccaPurple));
        yield return new((Color.Red, new Color(255, 0, 0)), nameof(Color.Red));
        yield return new((Color.RosyBrown, new Color(188, 143, 143)), nameof(Color.RosyBrown));
        yield return new((Color.RoyalBlue, new Color(65, 105, 225)), nameof(Color.RoyalBlue));
        yield return new((Color.SaddleBrown, new Color(139, 69, 19)), nameof(Color.SaddleBrown));
        yield return new((Color.Salmon, new Color(250, 128, 114)), nameof(Color.Salmon));
        yield return new((Color.SandyBrown, new Color(244, 164, 96)), nameof(Color.SandyBrown));
        yield return new((Color.SeaGreen, new Color(46, 139, 87)), nameof(Color.SeaGreen));
        yield return new((Color.SeaShell, new Color(255, 245, 238)), nameof(Color.SeaShell));
        yield return new((Color.Sienna, new Color(160, 82, 45)), nameof(Color.Sienna));
        yield return new((Color.Silver, new Color(192, 192, 192)), nameof(Color.Silver));
        yield return new((Color.SkyBlue, new Color(135, 206, 235)), nameof(Color.SkyBlue));
        yield return new((Color.SlateBlue, new Color(106, 90, 205)), nameof(Color.SlateBlue));
        yield return new((Color.SlateGray, new Color(112, 128, 144)), nameof(Color.SlateGray));
        yield return new((Color.Snow, new Color(255, 250, 250)), nameof(Color.Snow));
        yield return new((Color.SpringGreen, new Color(0, 255, 127)), nameof(Color.SpringGreen));
        yield return new((Color.SteelBlue, new Color(70, 130, 180)), nameof(Color.SteelBlue));
        yield return new((Color.Tan, new Color(210, 180, 140)), nameof(Color.Tan));
        yield return new((Color.Teal, new Color(0, 128, 128)), nameof(Color.Teal));
        yield return new((Color.Thistle, new Color(216, 191, 216)), nameof(Color.Thistle));
        yield return new((Color.Tomato, new Color(255, 99, 71)), nameof(Color.Tomato));
        yield return new((Color.Turquoise, new Color(64, 224, 208)), nameof(Color.Turquoise));
        yield return new((Color.Violet, new Color(238, 130, 238)), nameof(Color.Violet));
        yield return new((Color.Wheat, new Color(245, 222, 179)), nameof(Color.Wheat));
        yield return new((Color.White, new Color(255, 255, 255)), nameof(Color.White));
        yield return new((Color.WhiteSmoke, new Color(245, 245, 245)), nameof(Color.WhiteSmoke));
        yield return new((Color.Yellow, new Color(255, 255, 0)), nameof(Color.Yellow));
        yield return new((Color.YellowGreen, new Color(154, 205, 50)), nameof(Color.YellowGreen));
    }

    public static IEnumerable<(string Format, int Size)> TooSmallHexDestinations()
    {
        yield return ("X", 0);
        yield return ("X", 1);
        yield return ("X", 3);
        yield return ("X", 5);
        yield return ("X", 8);
        yield return ("x", 0);
        yield return ("x", 1);
        yield return ("x", 3);
        yield return ("x", 5);
        yield return ("x", 7);
    }
}
