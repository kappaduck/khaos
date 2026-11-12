// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting.Logging.Console;
using Microsoft.Extensions.Logging;

namespace KappaDuck.Khaos.Tests.Hosting.Logging.Console;

internal sealed class ConsoleQueueTests
{
    private static readonly ConsoleEntry Loaded = new(null, LogLevel.Information, "Level Forest loaded.", null);
    private static readonly ConsoleEntry Scored = new(null, LogLevel.Debug, "Score is now 10.", null);
    private static readonly ConsoleEntry Fell = new(null, LogLevel.Warning, "The ball fell off the table.", null);
    private static readonly ConsoleEntry Saved = new(null, LogLevel.Information, "Game saved.", null);

    private readonly GatedWriter _writer = new();

    [After(Test)]
    public void OpenTheGate() => _writer.Open();

    [Test]
    public async Task EnqueueShouldReachTheWriter()
    {
        _writer.Open();
        ConsoleQueue queue = new(_writer.Write, 10, ConsoleQueueFullMode.Wait);

        queue.Enqueue(in Loaded);
        queue.Dispose();

        await _writer.Written.Single().Should().BeEqualTo(Loaded);
    }

    [Test]
    public async Task EnqueueShouldKeepTheOrderOfTheLogs()
    {
        _writer.Open();
        ConsoleQueue queue = new(_writer.Write, 10, ConsoleQueueFullMode.Wait);

        queue.Enqueue(in Loaded);
        queue.Enqueue(in Scored);
        queue.Enqueue(in Fell);
        queue.Dispose();

        await _writer.Written.SequenceEqual([Loaded, Scored, Fell]).Should().BeTrue();
    }

    [Test]
    public async Task EnqueueShouldWriteRightAwayOnceDisposed()
    {
        _writer.Open();
        ConsoleQueue queue = new(_writer.Write, 10, ConsoleQueueFullMode.Wait);
        queue.Dispose();

        queue.Enqueue(in Loaded);

        await _writer.Written.Single().Should().BeEqualTo(Loaded);
    }

    [Test]
    public async Task EnqueueShouldWaitWhileTheQueueIsFull()
    {
        using ConsoleQueue queue = FullQueue(ConsoleQueueFullMode.Wait);

        Task blocked = Task.Run(() => queue.Enqueue(in Fell));
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        await blocked.IsCompleted.Should().BeFalse();
    }

    [Test]
    public async Task EnqueueShouldGoOnOnceTheWriterFreesAPlace()
    {
        ConsoleQueue queue = FullQueue(ConsoleQueueFullMode.Wait);
        Task blocked = Task.Run(() => queue.Enqueue(in Fell));

        _writer.Open();
        await blocked.WaitAsync(TimeSpan.FromSeconds(5));
        queue.Dispose();

        await _writer.Written.SequenceEqual([Loaded, Scored, Fell]).Should().BeTrue();
    }

    [Test]
    public async Task EnqueueShouldDropALogWhenTheQueueIsFull()
    {
        ConsoleQueue queue = FullQueue(ConsoleQueueFullMode.Drop);

        queue.Enqueue(in Fell);
        _writer.Open();
        queue.Dispose();

        await _writer.Written.SequenceEqual([Loaded, Scored]).Should().BeTrue();
    }

    [Test]
    public async Task EnqueueShouldReportTheDroppedLogsBeforeTheNextOne()
    {
        ConsoleQueue queue = FullQueue(ConsoleQueueFullMode.Drop);
        queue.Enqueue(in Fell);
        _writer.Open();
        _writer.WaitFor(2);

        queue.Enqueue(in Saved);
        queue.Dispose();

        ConsoleEntry dropped = _writer.Written[2];
        await dropped.Message.Should().BeEqualTo("The console could not keep up, so 1 log was dropped.");
    }

    private ConsoleQueue FullQueue(ConsoleQueueFullMode mode)
    {
        ConsoleQueue queue = new(_writer.Write, 1, mode);

        queue.Enqueue(in Loaded);
        _writer.WaitUntilWriting();
        queue.Enqueue(in Scored);

        return queue;
    }

    private sealed class GatedWriter
    {
        private readonly ManualResetEventSlim _gate = new();
        private readonly ManualResetEventSlim _writing = new();
        private readonly List<ConsoleEntry> _written = [];

        internal IReadOnlyList<ConsoleEntry> Written
        {
            get
            {
                lock (_written)
                    return [.. _written];
            }
        }

        internal void Open() => _gate.Set();

        internal void WaitFor(int count) => SpinWait.SpinUntil(() => Written.Count >= count, TimeSpan.FromSeconds(5));

        internal void WaitUntilWriting() => _writing.Wait(TimeSpan.FromSeconds(5));

        internal void Write(ConsoleEntry entry)
        {
            _writing.Set();
            _gate.Wait();

            lock (_written)
                _written.Add(entry);
        }
    }
}
