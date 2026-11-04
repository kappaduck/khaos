// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Mathematics;
using KappaDuck.Khaos.Tests.Helpers;
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

    private EngineHost _host = null!;

    [Before(Test)]
    public void StartEngine() => _host = EngineHost.Start(new() { Name = "Duck Quest" });

    [After(Test)]
    public void StopEngine() => _host.Dispose();

    [Test]
    public async Task OpenShouldUseTheTitle()
    {
        using WindowScope scope = WindowScope.Open(_options);

        await scope.Window.Title.Should().BeEqualTo("Duck Quest");
    }

    [Test]
    public async Task OpenShouldUseTheLogicalSize()
    {
        using WindowScope scope = WindowScope.Open(_options);

        await scope.Window.Size.Should().BeEqualTo(new SizeI(640, 360));
    }

    [Test]
    public async Task OpenShouldShowTheWindow()
    {
        using WindowScope scope = WindowScope.Open(_options);

        await scope.Window.Hidden.Should().BeFalse();
    }

    [Test]
    public async Task OpenShouldApplyResizable()
    {
        WindowOptions resizable = new() { Title = "Duck Quest", Width = 640, Height = 360, Resizable = true };

        using WindowScope scope = WindowScope.Open(resizable);

        await scope.Window.Resizable.Should().BeTrue();
    }

    [Test]
    public async Task OpenShouldApplyBorderless()
    {
        WindowOptions borderless = new() { Title = "Duck Quest", Width = 640, Height = 360, Borderless = true };

        using WindowScope scope = WindowScope.Open(borderless);

        await scope.Window.Borderless.Should().BeTrue();
    }

    [Test]
    public async Task OpenShouldRejectAnEmptySize()
    {
        WindowOptions empty = new() { Title = "Duck Quest", Width = 0 };

        await Assert.That(() => Window.Open(empty)).Throws<ArgumentOutOfRangeException>();
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
        using WindowScope scope = WindowScope.Open(_options);

        await scope.Window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task OpenShouldUseThePrimaryDisplayForAnUnpluggedDisplay()
    {
        WindowOptions unplugged = new() { Title = "Duck Quest", Width = 640, Height = 360, Display = new Display(0xFFFF) };

        using WindowScope scope = WindowScope.Open(unplugged);

        await scope.Window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    [MultipleScreensOnly]
    public async Task OpenShouldUseTheChosenDisplay()
    {
        Display second = SecondDisplay();
        WindowOptions onSecond = new() { Title = "Duck Quest", Width = 640, Height = 360, Display = second };

        using WindowScope scope = WindowScope.Open(onSecond);

        await scope.Window.Display.Should().BeEqualTo(second);
    }

    [Test]
    public async Task OpenShouldCenterTheWindowInTheUsableArea()
    {
        using WindowScope scope = WindowScope.Open(_options);
        RectI area = scope.Window.Display.UsableArea;
        SizeI size = scope.Window.Size;

        PointI position = scope.Window.Position;

        await ((float)position.X).Should().BeCloseTo(area.X + ((area.Width - size.Width) / 2f), 2f);
        await ((float)position.Y).Should().BeCloseTo(area.Y + ((area.Height - size.Height) / 2f), 2f);
    }

    [Test]
    [OffscreenOnly]
    public async Task CanMoveShouldBeFalseWithoutARealScreen()
    {
        using WindowScope scope = WindowScope.Open(_options);

        await scope.Window.CanMove.Should().BeFalse();
    }

    [Test]
    [RealScreenOnly]
    [RunOn(OS.Windows)]
    public async Task CanMoveShouldBeTrueOnWindows()
    {
        using WindowScope scope = WindowScope.Open(_options);

        await scope.Window.CanMove.Should().BeTrue();
    }

    [Test]
    [OffscreenOnly]
    public async Task PositionShouldDoNothingWhenTheWindowCannotMove()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Window window = scope.Window;

        await Assert.That(() => window.Position = new PointI(50, 60)).ThrowsNothing();
    }

    [Test]
    public async Task DisplayShouldUseThePrimaryScreenForTheDefaultDisplay()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Display = default;
        scope.Window.Sync();

        await scope.Window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    public async Task MoveToShouldUseThePrimaryScreenForAnUnpluggedDisplay()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Display unplugged = new(uint.MaxValue >> 16);

        scope.Window.MoveTo(unplugged, new PointI(50, 60));
        scope.Window.Sync();

        await scope.Window.Display.Should().BeEqualTo(Display.Primary);
    }

    [Test]
    [RealScreenOnly]
    public async Task OpenShouldUseTheChosenPosition()
    {
        WindowOptions saved = new() { Title = "Duck Quest", Width = 640, Height = 360, Position = new PointI(40, 30) };

        using WindowScope scope = WindowScope.Open(saved);

        await scope.Window.Position.Should().BeEqualTo(new PointI(40, 30));
    }

    [Test]
    [RealScreenOnly]
    public async Task PositionShouldMoveTheWindow()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Position = new PointI(50, 60);
        scope.Window.Sync();

        await scope.Window.Position.Should().BeEqualTo(new PointI(50, 60));
    }

    [Test]
    [MultipleScreensOnly]
    public async Task MoveToShouldPlaceTheWindowOnThatDisplay()
    {
        Display second = SecondDisplay();
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.MoveTo(second, new PointI(20, 20));
        scope.Window.Sync();

        await scope.Window.Display.Should().BeEqualTo(second);
        await scope.Window.Position.Should().BeEqualTo(new PointI(20, 20));
    }

    [Test]
    [MultipleScreensOnly]
    public async Task MoveToShouldKeepTheLogicalSizeOnAScreenWithAnotherScale()
    {
        Display second = SecondDisplay();
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.MoveTo(second, new PointI(20, 20));
        scope.Window.Sync();

        await scope.Window.Size.Should().BeEqualTo(new SizeI(640, 360));
    }

    [Test]
    [MultipleScreensOnly]
    public async Task DisplayShouldMoveTheWindowToAnotherDisplay()
    {
        Display second = SecondDisplay();
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Display = second;
        scope.Window.Sync();

        await scope.Window.Display.Should().BeEqualTo(second);
    }

    [Test]
    public async Task OpenShouldThrowWhenTheEngineIsNotStarted()
    {
        _host.Dispose();

        await Assert.That(static () => Window.Open(_options)).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task TitleShouldChangeTheNativeTitle()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Title = "Duck Quest - Level 2";

        await scope.Window.Title.Should().BeEqualTo("Duck Quest - Level 2");
    }

    [Test]
    public async Task SetTitleShouldUseOnlyTheCharactersOfTheSpan()
    {
        using WindowScope scope = WindowScope.Open(_options);
        ReadOnlySpan<char> level = "Duck Quest - Level 2 - Boss".AsSpan(0, 20);

        scope.Window.SetTitle(level);

        await scope.Window.Title.Should().BeEqualTo("Duck Quest - Level 2");
    }

    [Test]
    public async Task SetTitleShouldFormatAnInterpolatedString()
    {
        using WindowScope scope = WindowScope.Open(_options);
        const int level = 2;

        scope.Window.SetTitle($"Duck Quest - Level {level}");

        await scope.Window.Title.Should().BeEqualTo("Duck Quest - Level 2");
    }

    [Test]
    public async Task SetTitleShouldKeepCharactersOutsideAscii()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.SetTitle("Canard — Niveau 2 🦆");

        await scope.Window.Title.Should().BeEqualTo("Canard — Niveau 2 🦆");
    }

    [Test]
    public async Task SetTitleShouldAcceptATitleTooLongForTheStack()
    {
        using WindowScope scope = WindowScope.Open(_options);
        string longTitle = new('Q', 1000);

        scope.Window.SetTitle(longTitle);

        await scope.Window.Title.Should().BeEqualTo(longTitle);
    }

    [Test]
    public async Task SetTitleShouldAcceptAnEmptyTitle()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.SetTitle([]);

        await scope.Window.Title.Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task SizeShouldResizeTheWindow()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Size = new SizeI(800, 600);
        scope.Window.Sync();

        await scope.Window.Size.Should().BeEqualTo(new SizeI(800, 600));
    }

    [Test]
    public async Task SizeShouldRejectAnEmptySize()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Window window = scope.Window;

        await Assert.That(() => window.Size = new SizeI(0, 600)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task SizeInPixelsShouldBeTheSizeTimesThePixelDensity()
    {
        using WindowScope scope = WindowScope.Open(_options);

        SizeI expected = (scope.Window.Size * scope.Window.PixelDensity).Round();

        await scope.Window.SizeInPixels.Should().BeEqualTo(expected);
    }

    [Test]
    public async Task MinimumSizeShouldStoreTheLimit()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.MinimumSize = new SizeI(320, 180);

        await scope.Window.MinimumSize.Should().BeEqualTo(new SizeI(320, 180));
    }

    [Test]
    public async Task MaximumSizeShouldStoreTheLimit()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.MaximumSize = new SizeI(1920, 1080);

        await scope.Window.MaximumSize.Should().BeEqualTo(new SizeI(1920, 1080));
    }

    [Test]
    public async Task MinimumSizeShouldRejectANegativeSize()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Window window = scope.Window;

        await Assert.That(() => window.MinimumSize = new SizeI(-1, 180)).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    [RealScreenOnly]
    public async Task ResizableShouldChangeTheNativeFlag()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Resizable = true;

        await scope.Window.Resizable.Should().BeTrue();
    }

    [Test]
    public async Task FullscreenShouldEnterAndLeaveFullscreen()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Window window = scope.Window;

        window.Fullscreen = true;
        window.Sync();
        bool entered = window.Fullscreen;

        window.Fullscreen = false;
        window.Sync();
        bool left = !window.Fullscreen;

        await entered.Should().BeTrue();
        await left.Should().BeTrue();
    }

    [Test]
    public async Task HideShouldHideTheWindow()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Hide();

        await scope.Window.Hidden.Should().BeTrue();
    }

    [Test]
    public async Task ShowShouldShowAHiddenWindow()
    {
        using WindowScope scope = WindowScope.Open(_options);
        scope.Window.Hide();

        scope.Window.Show();

        await scope.Window.Hidden.Should().BeFalse();
    }

    [Test]
    [RealScreenOnly]
    public async Task MaximizeShouldMaximizeTheWindow()
    {
        WindowOptions resizable = new() { Title = "Duck Quest", Width = 640, Height = 360, Resizable = true };
        using WindowScope scope = WindowScope.Open(resizable);

        scope.Window.Maximize();
        scope.Window.Sync();

        await scope.Window.Maximized.Should().BeTrue();
    }

    [Test]
    [RealScreenOnly]
    public async Task FocusableShouldChangeTheNativeFlag()
    {
        using WindowScope scope = WindowScope.Open(_options);

        scope.Window.Focusable = false;

        await scope.Window.Focusable.Should().BeFalse();
    }

    [Test]
    public async Task OpacityShouldRejectAValueAboveOne()
    {
        using WindowScope scope = WindowScope.Open(_options);
        Window window = scope.Window;

        await Assert.That(() => window.Opacity = 1.5f).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task CloseShouldMakeEveryMemberThrow()
    {
        Window window = Window.Open(_options);

        window.Close();

        await Assert.That(() => window.Title).Throws<ObjectDisposedException>();
        await Assert.That(() => window.SetTitle("Duck Quest")).Throws<ObjectDisposedException>();
        await Assert.That(() => window.Size).Throws<ObjectDisposedException>();
        await Assert.That(window.Show).Throws<ObjectDisposedException>();
    }

    [Test]
    public async Task CloseShouldBeSafeToCallTwice()
    {
        Window window = Window.Open(_options);
        window.Close();

        await Assert.That(window.Close).ThrowsNothing();
    }

    [Test]
    public async Task ScaleShouldMatchThePixelDensityOfAWindowOnThatScreen()
    {
        using WindowScope scope = WindowScope.Open(new WindowOptions { Title = "Duck Quest", Width = 640, Height = 360 });

        float scale = scope.Window.Display.Scale;

        await scale.Should().BeCloseTo(scope.Window.PixelDensity, 0.001f);
    }

    private static Display SecondDisplay()
    {
        Display primary = Display.Primary;
        return Display.All.First(display => display != primary);
    }
}
