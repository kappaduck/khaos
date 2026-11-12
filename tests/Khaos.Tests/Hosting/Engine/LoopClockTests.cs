// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;

namespace KappaDuck.Khaos.Tests.Hosting.Engine;

internal sealed class LoopClockTests
{
    private const long OneSecond = SteppedClock.Frequency;
    private const long OneStep = SteppedClock.TicksPerStepAt60Hz;

    private readonly LoopClock _clock = new(new LoopOptions(), SteppedClock.Frequency);

    [Test]
    public async Task AdvanceShouldRunNoStepBeforeAStepOfTime()
    {
        int steps = _clock.Advance(OneStep - 1);

        await steps.Should().BeEqualTo(0);
    }

    [Test]
    public async Task AdvanceShouldRunOneStepAfterAStepOfTime()
    {
        int steps = _clock.Advance(OneStep);

        await steps.Should().BeEqualTo(1);
    }

    [Test]
    public async Task AdvanceShouldKeepTheTimeLeftForTheNextFrame()
    {
        _ = _clock.Advance(OneStep / 2);

        int steps = _clock.Advance(OneStep);

        await steps.Should().BeEqualTo(1);
    }

    [Test]
    public async Task AdvanceShouldRunSixtyStepsPerSecondOnA144HzScreen()
    {
        int steps = Enumerable.Range(1, 144).Sum(frame => _clock.Advance(frame * SteppedClock.TicksPerFrameAt144Hz));

        await steps.Should().BeEqualTo(60);
    }

    [Test]
    public async Task AdvanceShouldRunTwoStepsPerFrameOnA30HzScreen()
    {
        int[] steps = [.. Enumerable.Range(1, 30).Select(frame => _clock.Advance(frame * SteppedClock.TicksPerFrameAt30Hz))];

        await steps.Should().All(static step => step == 2);
    }

    [Test]
    public async Task AdvanceShouldCapALongFrameAtAQuarterOfASecond()
    {
        int steps = _clock.Advance(2 * OneSecond);

        await steps.Should().BeEqualTo(15);
    }

    [Test]
    public async Task AdvanceShouldFollowTheMaxFrameTime()
    {
        LoopClock clock = new(new LoopOptions { MaxFrameTime = 0.5 }, SteppedClock.Frequency);

        int steps = clock.Advance(2 * OneSecond);

        await steps.Should().BeEqualTo(30);
    }

    [Test]
    public async Task AdvanceShouldFollowTheFixedRate()
    {
        LoopClock clock = new(new LoopOptions { FixedRate = 120 }, SteppedClock.Frequency);

        int steps = clock.Advance(OneStep);

        await steps.Should().BeEqualTo(2);
    }

    [Test]
    public async Task AdvanceShouldIgnoreTimeGoingBackwards()
    {
        _ = _clock.Advance(OneStep / 2);

        int steps = _clock.Advance(0);

        await steps.Should().BeEqualTo(0);
    }

    [Test]
    public async Task AdvanceShouldNotDriftOverAnHour()
    {
        const int frames = 3_600 * 144;

        int steps = Enumerable.Range(1, frames).Sum(frame => _clock.Advance(frame * SteppedClock.TicksPerFrameAt144Hz));

        await steps.Should().BeEqualTo(3_600 * 60);
    }

    [Test]
    public async Task StartShouldMeasureTheFirstFrameFromNow()
    {
        _clock.Start(OneSecond);

        int steps = _clock.Advance(OneSecond + OneStep);

        await steps.Should().BeEqualTo(1);
    }

    [Test]
    public async Task AlphaShouldBeZeroRightOnAStep()
    {
        _ = _clock.Advance(OneStep);

        await _clock.Alpha.Should().BeEqualTo(0f);
    }

    [Test]
    public async Task AlphaShouldBeHalfHalfwayToTheNextStep()
    {
        _ = _clock.Advance(OneStep + (OneStep / 2));

        await _clock.Alpha.Should().BeEqualTo(0.5f);
    }

    [Test]
    public async Task NextStepShouldGiveTheFixedDelta()
    {
        GameTime step = _clock.NextStep();

        await step.Delta.Should().BeEqualTo(1f / 60);
    }

    [Test]
    public async Task NextStepShouldCountTheSteps()
    {
        _ = _clock.NextStep();

        GameTime step = _clock.NextStep();

        await step.Count.Should().BeEqualTo(2L);
    }

    [Test]
    public async Task NextStepShouldComputeTheTotalFromTheCount()
    {
        _ = _clock.NextStep();
        _ = _clock.NextStep();

        GameTime step = _clock.NextStep();

        await step.Total.Should().BeEqualTo(3.0 / 60);
    }

    [Test]
    public async Task FrameShouldGiveTheTimeSinceTheLastFrame()
    {
        _ = _clock.Advance(SteppedClock.TicksPerFrameAt144Hz);

        await _clock.Frame.Delta.Should().BeEqualTo(1f / 144);
    }

    [Test]
    public async Task FrameShouldCapTheDeltaOfALongFrame()
    {
        _ = _clock.Advance(2 * OneSecond);

        await _clock.Frame.Delta.Should().BeEqualTo(0.25f);
    }

    [Test]
    public async Task FrameShouldAddUpTheTotal()
    {
        _ = _clock.Advance(OneSecond / 4);
        _ = _clock.Advance(OneSecond / 2);

        await _clock.Frame.Total.Should().BeEqualTo(0.5);
    }

    [Test]
    public async Task FrameShouldCountTheFrames()
    {
        _ = _clock.Advance(OneStep);
        _ = _clock.Advance(2 * OneStep);

        await _clock.Frame.Count.Should().BeEqualTo(2L);
    }

    [Test]
    public async Task ResumeShouldNotCountThePause()
    {
        _ = _clock.Advance(OneStep);
        _clock.Resume(10 * OneSecond);

        int steps = _clock.Advance((10 * OneSecond) + OneStep);

        await steps.Should().BeEqualTo(1);
    }

    [Test]
    public async Task ResumeShouldKeepTheTimeLeftBeforeThePause()
    {
        _ = _clock.Advance(OneStep / 2);
        _clock.Resume(10 * OneSecond);

        int steps = _clock.Advance((10 * OneSecond) + (OneStep / 2));

        await steps.Should().BeEqualTo(1);
    }

    [Test]
    public async Task ResumeShouldGiveUpdateTheTimeSinceTheResume()
    {
        _ = _clock.Advance(OneStep);
        _clock.Resume(10 * OneSecond);

        _ = _clock.Advance((10 * OneSecond) + SteppedClock.TicksPerFrameAt144Hz);

        await _clock.Frame.Delta.Should().BeEqualTo(1f / 144);
    }
}
