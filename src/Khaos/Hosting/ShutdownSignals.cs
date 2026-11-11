// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Hosting;

[UnsupportedOSPlatform("browser")]
internal sealed class ShutdownSignals : IDisposable
{
    private readonly PosixSignalRegistration _interrupt;
    private readonly PosixSignalRegistration _terminate;

    private int _received;

    internal ShutdownSignals()
    {
        _interrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, Handle);
        _terminate = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Handle);
    }

    public void Dispose()
    {
        _interrupt.Dispose();
        _terminate.Dispose();
    }

    internal void Handle(PosixSignalContext context)
    {
        if (Interlocked.Exchange(ref _received, 1) != 0)
            return;

        SDL_Event quit = new(SDL_EventType.Quit);
        context.Cancel = SDL3.PushEvent(ref quit);
    }
}
