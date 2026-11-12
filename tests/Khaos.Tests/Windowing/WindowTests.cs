// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TUnit.Core.Enums;

namespace KappaDuck.Khaos.Tests.Windowing;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class WindowTests
{
    private static readonly WindowOptions _options = new() { Title = "Duck Quest", Width = 640, Height = 360 };

    private readonly Window _window = new();

    private NativeSession _session = null!;

    [Before(Test)]
    public void StartEngine() => _session = NativeSession.Start(new AppMetadata() { Name = "Duck Quest" });

    [After(Test)]
    public void StopEngine()
    {
        _window.Close();
        _session.Dispose();
    }

    [Test]
    public async Task OpenShouldUseTheTitle()
    {
        _window.Open(_options);

        await _window.Title.Should().BeEqualTo("Duck Quest");
    }

    [Test]
    public async Task OpenShouldPreferTheTitleOverTheDefaultTitle()
    {
        _window.Open(_options, "Duck Quest Deluxe");

        await _window.Title.Should().BeEqualTo("Duck Quest");
    }

    [Test]
    public async Task OpenShouldUseTheDefaultTitleWhenTheTitleIsEmpty()
    {
        WindowOptions untitled = new() { Width = 640, Height = 360 };

        _window.Open(untitled, "Duck Quest Deluxe");

        await _window.Title.Should().BeEqualTo("Duck Quest Deluxe");
    }

    [Test]
    public async Task OpenShouldUseTheLogicalSize()
    {
        _window.Open(_options);

        await _window.Size.Should().BeEqualTo(new SizeI(640, 360));
    }

    [Test]
    public async Task OpenShouldShowTheWindow()
    {
        _window.Open(_options);

        await _window.Hidden.Should().BeFalse();
    }

    [Test]
    public async Task OpenShouldApplyResizable()
    {
        WindowOptions resizable = new() { Title = "Duck Quest", Width = 640, Height = 360, Resizable = true };

        _window.Open(resizable);

        await _window.Resizable.Should().BeTrue();
    }

    [Test]
    public async Task OpenShouldApplyFullscreen()
    {
        WindowOptions fullscreen = new() { Title = "Duck Quest", Width = 640, Height = 360, Fullscreen = true };

        _window.Open(fullscreen);

        await _window.Fullscreen.Should().BeTrue();
    }

    [Test]
    public async Task OpenShouldApplyBorderless()
    {
        WindowOptions borderless = new() { Title = "Duck Quest", Width = 640, Height = 360, Borderless = true };

        _window.Open(borderless);

        await _window.Borderless.Should().BeTrue();
    }

    [Test]
    public async Task OpenShouldRejectAnEmptySize()
    {
        WindowOptions empty = new() { Title = "Duck Quest", Width = 0 };

        await Assert.That(() => _window.Open(empty)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ConfigureShouldReadTheScreensWhenTheEngineIsStarted()
    {
        ServiceCollection services = new();
        services.Configure<WindowOptions>(static window => window.Display = Display.Primary);

        WindowOptions options = services.BuildServiceProvider().GetRequiredService<IOptions<WindowOptions>>().Value;

        await options.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task OpenShouldUseThePrimaryDisplayByDefault()
    {
        _window.Open(_options);

        await _window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task OpenShouldUseThePrimaryDisplayForAnUnpluggedDisplay()
    {
        WindowOptions unplugged = new() { Title = "Duck Quest", Width = 640, Height = 360, Display = new Display(0xFFFF) };

        _window.Open(unplugged);

        await _window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task OpenShouldUseTheChosenDisplayWhenItIsConnected()
    {
        WindowOptions onPrimary = new() { Title = "Duck Quest", Width = 640, Height = 360, Display = Display.Primary };

        _window.Open(onPrimary);

        await _window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    [OffscreenOnly]
    public async Task OpenShouldIgnoreTheChosenPositionWhenTheWindowCannotMove()
    {
        WindowOptions saved = new() { Title = "Duck Quest", Width = 640, Height = 360, Position = new PointI(40, 30) };

        await Assert.That(() => _window.Open(saved)).ThrowsNothing();
    }

    [Test]
    [MultipleScreensOnly]
    public async Task OpenShouldUseTheChosenDisplay()
    {
        Display second = SecondDisplay();
        WindowOptions onSecond = new() { Title = "Duck Quest", Width = 640, Height = 360, Display = second };

        _window.Open(onSecond);

        await _window.Display.Should().BeEqualTo(second);
    }

    [Test]
    public async Task OpenShouldCenterTheWindowInTheUsableArea()
    {
        _window.Open(_options);
        RectI area = _window.Display.UsableArea;
        SizeI size = _window.Size;

        PointI position = _window.Position;

        await ((float)position.X).Should().BeCloseTo(area.X + ((area.Width - size.Width) / 2f), 2f);
        await ((float)position.Y).Should().BeCloseTo(area.Y + ((area.Height - size.Height) / 2f), 2f);
    }

    [Test]
    [OffscreenOnly]
    public async Task CanMoveShouldReturnFalseWithoutARealScreen()
    {
        _window.Open(_options);

        await _window.CanMove.Should().BeFalse();
    }

    [Test]
    [RealScreenOnly]
    [RunOn(OS.Windows)]
    public async Task CanMoveShouldReturnTrueOnWindows()
    {
        _window.Open(_options);

        await _window.CanMove.Should().BeTrue();
    }

    [Test]
    [OffscreenOnly]
    public async Task PositionShouldDoNothingWhenTheWindowCannotMove()
    {
        _window.Open(_options);
        await Assert.That(() => _window.Position = new PointI(50, 60)).ThrowsNothing();
    }

    [Test]
    public async Task DisplayShouldUseThePrimaryScreenForTheDefaultDisplay()
    {
        _window.Open(_options);

        _window.Display = default;
        _window.Sync();

        await _window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task MoveToShouldUseThePrimaryScreenForAnUnpluggedDisplay()
    {
        _window.Open(_options);
        Display unplugged = new(uint.MaxValue >> 16);

        _window.MoveTo(unplugged, new PointI(50, 60));
        _window.Sync();

        await _window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    [RealScreenOnly]
    public async Task OpenShouldUseTheChosenPosition()
    {
        WindowOptions saved = new() { Title = "Duck Quest", Width = 640, Height = 360, Position = new PointI(40, 30) };

        _window.Open(saved);

        await _window.Position.Should().BeEqualTo(new PointI(40, 30));
    }

    [Test]
    [RealScreenOnly]
    public async Task PositionShouldMoveTheWindow()
    {
        _window.Open(_options);

        _window.Position = new PointI(50, 60);
        _window.Sync();

        await _window.Position.Should().BeEqualTo(new PointI(50, 60));
    }

    [Test]
    [MultipleScreensOnly]
    public async Task MoveToShouldPlaceTheWindowOnThatDisplay()
    {
        Display second = SecondDisplay();
        _window.Open(_options);

        _window.MoveTo(second, new PointI(20, 20));
        _window.Sync();

        await _window.Display.Should().BeEqualTo(second);
        await _window.Position.Should().BeEqualTo(new PointI(20, 20));
    }

    [Test]
    [MultipleScreensOnly]
    public async Task MoveToShouldKeepTheLogicalSizeOnAScreenWithAnotherScale()
    {
        Display second = SecondDisplay();
        _window.Open(_options);

        _window.MoveTo(second, new PointI(20, 20));
        _window.Sync();

        await _window.Size.Should().BeEqualTo(new SizeI(640, 360));
    }

    [Test]
    [MultipleScreensOnly]
    public async Task DisplayShouldMoveTheWindowToAnotherDisplay()
    {
        Display second = SecondDisplay();
        _window.Open(_options);

        _window.Display = second;
        _window.Sync();

        await _window.Display.Should().BeEqualTo(second);
    }

    [Test]
    public async Task OpenShouldThrowWhenTheEngineIsNotStarted()
    {
        _session.Dispose();

        await Assert.That(() => _window.Open(_options)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TitleShouldChangeTheNativeTitle()
    {
        _window.Open(_options);

        _window.Title = "Duck Quest - Level 2";

        await _window.Title.Should().BeEqualTo("Duck Quest - Level 2");
    }

    [Test]
    public async Task SetTitleShouldUseOnlyTheCharactersOfTheSpan()
    {
        _window.Open(_options);
        ReadOnlySpan<char> level = "Duck Quest - Level 2 - Boss".AsSpan(0, 20);

        _window.SetTitle(level);

        await _window.Title.Should().BeEqualTo("Duck Quest - Level 2");
    }

    [Test]
    public async Task SetTitleShouldFormatAnInterpolatedString()
    {
        _window.Open(_options);
        const int level = 2;

        _window.SetTitle($"Duck Quest - Level {level}");

        await _window.Title.Should().BeEqualTo("Duck Quest - Level 2");
    }

    [Test]
    public async Task SetTitleShouldKeepCharactersOutsideAscii()
    {
        _window.Open(_options);

        _window.SetTitle("Canard — Niveau 2 🦆");

        await _window.Title.Should().BeEqualTo("Canard — Niveau 2 🦆");
    }

    [Test]
    public async Task SetTitleShouldAcceptATitleTooLongForTheStack()
    {
        _window.Open(_options);
        string longTitle = new('Q', 1000);

        _window.SetTitle(longTitle);

        await _window.Title.Should().BeEqualTo(longTitle);
    }

    [Test]
    public async Task SetTitleShouldAcceptAnEmptyTitle()
    {
        _window.Open(_options);

        _window.SetTitle([]);

        await _window.Title.Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task SizeShouldResizeTheWindow()
    {
        _window.Open(_options);

        _window.Size = new SizeI(800, 600);
        _window.Sync();

        await _window.Size.Should().BeEqualTo(new SizeI(800, 600));
    }

    [Test]
    public async Task SizeShouldRejectAnEmptySize()
    {
        _window.Open(_options);
        await Assert.That(() => _window.Size = new SizeI(0, 600)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task SizeInPixelsShouldBeTheSizeTimesThePixelDensity()
    {
        _window.Open(_options);

        SizeI expected = (_window.Size * _window.PixelDensity).Round();

        await _window.SizeInPixels.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task MinimumSizeShouldStoreTheLimit()
    {
        _window.Open(_options);

        _window.MinimumSize = new SizeI(320, 180);

        await _window.MinimumSize.Should().BeEqualTo(new SizeI(320, 180));
    }

    [Test]
    public async Task MaximumSizeShouldStoreTheLimit()
    {
        _window.Open(_options);

        _window.MaximumSize = new SizeI(1920, 1080);

        await _window.MaximumSize.Should().BeEqualTo(new SizeI(1920, 1080));
    }

    [Test]
    public async Task MinimumSizeShouldRejectANegativeSize()
    {
        _window.Open(_options);
        await Assert.That(() => _window.MinimumSize = new SizeI(-1, 180)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    [RealScreenOnly]
    public async Task ResizableShouldChangeTheNativeFlag()
    {
        _window.Open(_options);

        _window.Resizable = true;

        await _window.Resizable.Should().BeTrue();
    }

    [Test]
    public async Task FullscreenShouldEnterAndLeaveFullscreen()
    {
        _window.Open(_options);
        _window.Fullscreen = true;
        _window.Sync();
        bool entered = _window.Fullscreen;

        _window.Fullscreen = false;
        _window.Sync();
        bool left = !_window.Fullscreen;

        await entered.Should().BeTrue();
        await left.Should().BeTrue();
    }

    [Test]
    public async Task HideShouldHideTheWindow()
    {
        _window.Open(_options);

        _window.Hide();

        await _window.Hidden.Should().BeTrue();
    }

    [Test]
    public async Task ShowShouldShowAHiddenWindow()
    {
        _window.Open(_options);
        _window.Hide();

        _window.Show();

        await _window.Hidden.Should().BeFalse();
    }

    [Test]
    [RealScreenOnly]
    public async Task MaximizeShouldMaximizeTheWindow()
    {
        WindowOptions resizable = new() { Title = "Duck Quest", Width = 640, Height = 360, Resizable = true };
        _window.Open(resizable);

        _window.Maximize();
        _window.Sync();

        await _window.Maximized.Should().BeTrue();
    }

    [Test]
    [RealScreenOnly]
    public async Task FocusableShouldChangeTheNativeFlag()
    {
        _window.Open(_options);

        _window.Focusable = false;

        await _window.Focusable.Should().BeFalse();
    }

    [Test]
    public async Task OpacityShouldRejectAValueAboveOne()
    {
        _window.Open(_options);
        await Assert.That(() => _window.Opacity = 1.5f).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task OpacityShouldRejectNaN()
    {
        _window.Open(_options);
        await Assert.That(() => _window.Opacity = float.NaN).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task OpacityShouldRejectANaNWithoutItsSignBit()
    {
        _window.Open(_options);
        float positiveNaN = BitConverter.Int32BitsToSingle(0x7FC00000);

        await Assert.That(() => _window.Opacity = positiveNaN).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task OpacityShouldBe1ByDefault()
    {
        _window.Open(_options);

        await _window.Opacity.Should().BeEqualTo(1f);
    }

    [Test]
    [OffscreenOnly]
    public async Task OpacityShouldThrowWhenTheScreenCannotChangeIt()
    {
        _window.Open(_options);
        await Assert.That(() => _window.Opacity = 0.5f).Throws<PlatformException>();
    }

    [Test]
    public async Task FocusableShouldReturnTrueByDefault()
    {
        _window.Open(_options);

        await _window.Focusable.Should().BeTrue();
    }

    [Test]
    public async Task MinimizedShouldReturnFalseForANewWindow()
    {
        _window.Open(_options);

        await _window.Minimized.Should().BeFalse();
    }

    [Test]
    public async Task MaximizedShouldReturnFalseForANewWindow()
    {
        _window.Open(_options);

        await _window.Maximized.Should().BeFalse();
    }

    [Test]
    [OffscreenOnly]
    public async Task OccludedShouldReturnFalseWithoutARealScreen()
    {
        _window.Open(_options);

        await _window.Occluded.Should().BeFalse();
    }

    [Test]
    public async Task CloseShouldMakeEveryMemberThrow()
    {
        _window.Open(_options);

        _window.Close();

        await Assert.That(() => _window.Title).Throws<InvalidOperationException>();
        await Assert.That(() => _window.SetTitle("Duck Quest")).Throws<InvalidOperationException>();
        await Assert.That(() => _window.Size).Throws<InvalidOperationException>();
        await Assert.That(_window.Show).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task CloseShouldSayTheWindowIsClosed()
    {
        _window.Open(_options);

        _window.Close();

        await Assert.That(() => _window.Title).Throws<InvalidOperationException>().WithMessage("The window is closed: the game has exited.");
    }

    [Test]
    public async Task CloseShouldBeSafeToCallTwice()
    {
        _window.Open(_options);
        _window.Close();

        await Assert.That(_window.Close).ThrowsNothing();
    }

    [Test]
    public async Task WindowShouldSayItIsNotOpenYetBeforeItOpens()
    {
        await Assert.That(() => _window.Title).Throws<InvalidOperationException>().WithMessage(Window.NotOpenYet);
    }

    [Test]
    public async Task OpenShouldThrowWhenTheWindowWasAlreadyOpened()
    {
        _window.Open(_options);
        _window.Close();

        await Assert.That(() => _window.Open(_options)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ScaleShouldMatchThePixelDensityOfAWindowOnThatScreen()
    {
        _window.Open(new WindowOptions { Title = "Duck Quest", Width = 640, Height = 360 });

        float scale = _window.Display.Scale;

        await scale.Should().BeCloseTo(_window.PixelDensity, 0.001f);
    }

    private static Display SecondDisplay()
    {
        Display primary = Display.Primary;
        return Display.All.First(display => display != primary);
    }
}
