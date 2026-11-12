// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;

namespace KappaDuck.Khaos.Tests.Windowing;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class DisplayTests
{
    private const int OffscreenHeight = 768;
    private const int OffscreenWidth = 1024;
    private const int UnpluggedId = 0xFFF;

    private NativeSession _session = null!;

    [Before(Test)]
    public void StartEngine() => _session = NativeSession.Start(new AppMetadata { Name = "Display " });

    [After(Test)]
    public void StopEngine() => _session.Dispose();

    [Test]
    public async Task ConnectedShouldReturnFalseForAScreenThatIsNotConnected()
    {
        Display unplugged = new(UnpluggedId);

        await unplugged.Connected.Should().BeFalse();
    }

    [Test]
    public async Task ConnectedShouldReturnTrueForThePrimaryScreen()
    {
        Display display = Display.Primary;

        await display.Connected.Should().BeTrue();
    }

    [Test]
    public async Task NameShouldNotBeEmpty()
    {
        Display display = Display.Primary;

        await display.Name.Should().NotBeEmpty();
    }

    [Test]
    public async Task NameShouldThrowForAScreenThatIsNotConnected()
    {
        Display unplugged = new(UnpluggedId);

        await Assert.That(() => unplugged.Name).Throws<PlatformException>();
    }

    [Test]
    [OffscreenOnly]
    public async Task KeyShouldBeTheNameForASingleScreen()
    {
        Display primary = Display.Primary;

        await primary.Key.Should().BeEqualTo(primary.Name);
    }

    [Test]
    [OffscreenOnly]
    public async Task RefreshRateShouldBe0WhenTheSystemDoesNotSay()
    {
        Display display = Display.Primary;

        await display.RefreshRate.Should().BeEqualTo(0f);
    }

    [Test]
    [OffscreenOnly]
    public async Task ScaleShouldBe1()
    {
        Display display = Display.Primary;

        await display.Scale.Should().BeEqualTo(1f);
    }

    [Test]
    [OffscreenOnly]
    public async Task SizeInPixelsShouldBeTheSizeAtAScaleOf1()
    {
        SizeI pixels = Display.Primary.SizeInPixels;

        await pixels.Should().BeEqualTo(new SizeI(OffscreenWidth, OffscreenHeight));
    }

    [Test]
    [OffscreenOnly]
    public async Task SizeShouldBeTheSizeOfTheScreen()
    {
        SizeI size = Display.Primary.Size;

        await size.Should().BeEqualTo(new SizeI(OffscreenWidth, OffscreenHeight));
    }

    [Test]
    [OffscreenOnly]
    public async Task UsableAreaShouldBeTheWholeScreenWhenTheSystemDoesNotSay()
    {
        RectI area = Display.Primary.UsableArea;

        await area.Should().BeEqualTo(new RectI(0, 0, OffscreenWidth, OffscreenHeight));
    }

    [Test]
    [OffscreenOnly]
    public async Task UsableAreaInPixelsShouldBeTheUsableAreaAtAScaleOf1()
    {
        RectI pixels = Display.Primary.UsableAreaInPixels;

        await pixels.Should().BeEqualTo(new RectI(0, 0, OffscreenWidth, OffscreenHeight));
    }

    [Test]
    public async Task UsableAreaShouldFitInsideEveryScreen()
    {
        IReadOnlyList<Display> screens = Display.All;

        bool everyAreaFits = screens.All(static screen => FitsInside(screen.UsableArea, screen.Size));

        await everyAreaFits.Should().BeTrue();
    }

    [Test]
    public async Task AllShouldContainThePrimaryScreen()
    {
        bool found = Display.All.Contains(Display.Primary);

        await found.Should().BeTrue();
    }

    [Test]
    [OffscreenOnly]
    public async Task AllShouldHoldOnlyThePrimaryScreen()
    {
        IReadOnlyList<Display> all = Display.All;

        await all.Count.Should().BeEqualTo(1);
        await all[0].Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task PrimaryShouldThrowWhenTheEngineIsNotStarted()
    {
        _session.Dispose();

        await Assert.That(static () => Display.Primary).Throws<PlatformException>();
    }

    [Test]
    public async Task EqualsShouldReturnTrueForTheSameScreen()
    {
        Display display = Display.Primary;

        bool result = display.Equals(Display.Primary);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsShouldReturnFalseForAnotherScreen()
    {
        Display display = Display.Primary;

        bool result = display.Equals(new Display(UnpluggedId));

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnTrueForTheSameScreenBoxed()
    {
        Display display = Display.Primary;
        object boxed = Display.Primary;

        bool result = display.Equals(boxed);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForNull()
    {
        Display display = Display.Primary;

        bool result = display.Equals(null);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task EqualsObjectShouldReturnFalseForAnotherType()
    {
        Display display = Display.Primary;
        object key = display.Key;

        bool result = display.Equals(key);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task GetHashCodeShouldMatchForTheSameScreen()
    {
        int expected = Display.Primary.GetHashCode();

        int hash = Display.Primary.GetHashCode();

        await hash.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task EqualityShouldReturnTrueForTheSameScreen()
    {
        Display display = Display.Primary;

        bool result = display == Display.Primary;

        await result.Should().BeTrue();
    }

    [Test]
    public async Task EqualityShouldReturnFalseForAnotherScreen()
    {
        Display display = Display.Primary;

        bool result = display == new Display(UnpluggedId);

        await result.Should().BeFalse();
    }

    [Test]
    public async Task InequalityShouldReturnTrueForAnotherScreen()
    {
        bool result = Display.Primary != new Display(UnpluggedId);

        await result.Should().BeTrue();
    }

    [Test]
    public async Task InequalityShouldReturnFalseForTheSameScreen()
    {
        Display primary = Display.Primary;

        bool result = primary != Display.Primary;

        await result.Should().BeFalse();
    }

    [Test]
    public async Task FindShouldGiveBackEveryScreenFromItsKey()
    {
        IReadOnlyList<Display> screens = Display.All;

        bool everyKeyFindsItsScreen = screens.All(static screen => Display.Find(screen.Key) == screen);

        await everyKeyFindsItsScreen.Should().BeTrue();
    }

    [Test]
    public async Task FindShouldGiveNullForAnUnknownName()
    {
        Display? display = Display.Find("No Such Screen");

        await display.Should().BeNull();
    }

    [Test]
    [OffscreenOnly]
    public async Task FindShouldGiveTheRemainingScreenWhenTheSecondIsGone()
    {
        Display primary = Display.Primary;

        bool found = Display.Find($"{primary.Name} #2") == primary;

        await found.Should().BeTrue();
    }

    [Test]
    [OffscreenOnly]
    public async Task UsableAreaShouldLeaveOutWhatTheSystemReserves()
    {
        const string TaskbarOf40 = "0,0,1024,728";
        const string UsableBoundsHint = NativeNames.DisplayUsableBounds;
        SDLHints.Set(UsableBoundsHint, TaskbarOf40);

        RectI area = Display.Primary.UsableArea;

        await area.Should().BeEqualTo(new RectI(0, 0, OffscreenWidth, OffscreenHeight - 40));

        SDLHints.Reset(UsableBoundsHint);
    }

    private static bool FitsInside(RectI area, SizeI screen) => new RectI(PointI.Origin, screen).Contains(area);
}
