// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Tests.Helpers;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Hosting;

[Category(Categories.Integration)]
[NotInParallel(Sequential.Engine)]
internal sealed class ShutdownSignalsTests
{
    private NativeSession _session = null!;
    private ShutdownSignals _signals = null!;

    [Before(Test)]
    public void StartEngine()
    {
        _session = NativeSession.Start(new AppMetadata() { Name = "Duck Quest" });
        _signals = new ShutdownSignals();
    }

    [After(Test)]
    public void StopEngine()
    {
        _signals.Dispose();
        _session.Dispose();
    }

    [Test]
    public async Task HandleShouldCancelTheFirstSignal()
    {
        PosixSignalContext interrupt = new(PosixSignal.SIGINT);

        _signals.Handle(interrupt);

        await interrupt.Cancel.Should().BeTrue();
    }

    [Test]
    public async Task HandleShouldPushAQuitEventForTheFirstSignal()
    {
        _signals.Handle(new PosixSignalContext(PosixSignal.SIGTERM));

        await SDLEvents.CountQuit().Should().BeEqualTo(1);
    }

    [Test]
    public async Task HandleShouldLetTheSecondSignalEndTheProcess()
    {
        _signals.Handle(new PosixSignalContext(PosixSignal.SIGINT));
        PosixSignalContext second = new(PosixSignal.SIGINT);

        _signals.Handle(second);

        await second.Cancel.Should().BeFalse();
    }

    [Test]
    public async Task HandleShouldPushOneQuitEventForTwoSignals()
    {
        _signals.Handle(new PosixSignalContext(PosixSignal.SIGINT));
        _signals.Handle(new PosixSignalContext(PosixSignal.SIGINT));

        await SDLEvents.CountQuit().Should().BeEqualTo(1);
    }
}
