// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Engine;
using KappaDuck.Khaos.Hosting.Logging;
using KappaDuck.Khaos.Interop.SDL.Primitives;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Logging;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class NativeLogTests
{
    private const string Pond = "The duck left the pond.";

    private readonly Mock<Game2D> _game = Game2D.Mock("Native", 320, 180);
    private readonly LogRecorder _logs = new();

    [Test]
    public async Task NativeLogShouldMapThePriorityToTheLevel()
    {
        using ILoggerFactory loggers = _logs.Factory(LogLevel.Debug);

        using (KhaosEngine _ = TestEngine.Start(_game, loggers))
            SDLLog.WriteWithoutFormatArguments(SDL_LogCategory.Render, SDL_LogPriority.Debug, Pond);

        await Assert.That(_logs.Entries).Contains(new RecordedLog($"{NativeLog.Category}.Render", LogLevel.Debug, Pond));
    }

    [Test]
    public async Task NativeLogShouldReachTheLoggerOfItsCategory()
    {
        using ILoggerFactory loggers = _logs.Factory(LogLevel.Information);

        using (KhaosEngine _ = TestEngine.Start(_game, loggers))
            SDLLog.WriteWithoutFormatArguments(SDL_LogCategory.Video, SDL_LogPriority.Warn, Pond);

        await Assert.That(_logs.Entries).Contains(new RecordedLog($"{NativeLog.Category}.Video", LogLevel.Warning, Pond));
    }

    [Test]
    public async Task NativeLogShouldReturnToTheDefaultOutputAfterTheGame()
    {
        using ILoggerFactory loggers = _logs.Factory(LogLevel.Information);
        TestEngine.Start(_game, loggers).Dispose();

        await SDLLog.OutputFunction().Should().BeEqualTo(SDLLog.DefaultOutputFunction());
    }

    [Test]
    public async Task NativeLogShouldSkipLevelsTheSettingsTurnOff()
    {
        using ILoggerFactory loggers = _logs.Factory(LogLevel.Warning);
        using KhaosEngine _ = TestEngine.Start(_game, loggers);

        await SDLLog.PriorityOf(SDL_LogCategory.Video).Should().BeEqualTo(SDL_LogPriority.Warn);
    }

    [Test]
    public async Task StartShouldLogTheVersionsOnce()
    {
        using ILoggerFactory loggers = _logs.Factory(LogLevel.Information);
        TestEngine.Start(_game, loggers).Dispose();

        await _logs.Entries.Count(static log => log.Category == KhaosEngine.Category).Should().BeEqualTo(1);
    }
}
