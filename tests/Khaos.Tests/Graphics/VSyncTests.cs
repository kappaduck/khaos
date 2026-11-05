// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;

namespace KappaDuck.Khaos.Tests.Graphics;

internal sealed class VSyncTests
{
    [Test]
    public async Task DefaultShouldBeDisabled()
    {
        VSync vsync = default;

        await vsync.Should().BeEqualTo(VSync.Disabled);
    }

    [Test]
    public async Task EveryShouldGiveEnabledForEveryRefresh()
    {
        VSync vsync = VSync.Every(1);

        await vsync.Should().BeEqualTo(VSync.Enabled);
    }

    [Test]
    public async Task EveryShouldKeepTheNumberOfRefreshes()
    {
        VSync vsync = VSync.Every(2);

        await vsync.Should().BeEqualTo(VSync.Every(2));
        await (vsync != VSync.Enabled).Should().BeTrue();
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public async Task EveryShouldRejectLessThanOneRefresh(int refreshes)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(() => VSync.Every(refreshes));

        await exception.ParamName.Should().BeEqualTo("refreshes");
    }

    [Test]
    public async Task EqualsObjectShouldReturnTrueForTheSameVSyncBoxed()
    {
        object boxed = VSync.Every(2);

        bool result = VSync.Every(2).Equals(boxed);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForNull()
    {
        bool result = VSync.Enabled.Equals(null);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForAnotherType()
    {
        object interval = VSync.Enabled.Interval;

        bool result = VSync.Enabled.Equals(interval);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task GetHashCodeShouldMatchForEqualVSyncs()
    {
        int expected = VSync.Every(3).GetHashCode();

        int hash = VSync.Every(3).GetHashCode();

        await hash.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task ToStringShouldNameTheVSync()
    {
        await VSync.Disabled.ToString().Should().BeEqualTo("Disabled");
        await VSync.Enabled.ToString().Should().BeEqualTo("Enabled");
        await VSync.Adaptive.ToString().Should().BeEqualTo("Adaptive");
        await VSync.Every(3).ToString().Should().BeEqualTo("Every(3)");
    }

    [Test]
    [Arguments("true", 1)]
    [Arguments("True", 1)]
    [Arguments("false", 0)]
    [Arguments("adaptive", -1)]
    [Arguments("ADAPTIVE", -1)]
    [Arguments("0", 0)]
    [Arguments("1", 1)]
    [Arguments("2", 2)]
    [Arguments(" 2 ", 2)]
    public async Task TryParseShouldReadASettingsValue(string value, int expected)
    {
        bool parsed = VSync.TryParse(value, out VSync vsync);

        await parsed.Should().BeTrue();
        await vsync.Interval.Should().BeEqualTo(expected);
    }

    [Test]
    [Arguments("maybe")]
    [Arguments("-1")]
    [Arguments("")]
    [Arguments("1.5")]
    public async Task TryParseShouldReturnFalseForAnUnknownValue(string value)
    {
        bool parsed = VSync.TryParse(value, out _);

        await parsed.Should().BeFalse();
    }
}
