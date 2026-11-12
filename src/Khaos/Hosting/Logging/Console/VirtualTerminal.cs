// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.Win32;
using KappaDuck.Khaos.Interop.Win32.Primitives;
using System.Runtime.Versioning;

namespace KappaDuck.Khaos.Hosting.Logging.Console;

[UnsupportedOSPlatform("browser")]
internal sealed class VirtualTerminal : IDisposable
{
    private readonly uint? _errorModeToRestore;
    private readonly uint? _outputModeToRestore;

    private VirtualTerminal(bool onOutput, bool onError, uint? outputModeToRestore, uint? errorModeToRestore)
    {
        OnOutput = onOutput;
        OnError = onError;
        _outputModeToRestore = outputModeToRestore;
        _errorModeToRestore = errorModeToRestore;
    }

    internal bool OnError { get; }

    internal bool OnOutput { get; }

    public void Dispose()
    {
        if (!OperatingSystem.IsWindows())
            return;

        Restore(Kernel32.StandardOutput, _outputModeToRestore);
        Restore(Kernel32.StandardError, _errorModeToRestore);
    }

    internal static VirtualTerminal Enable()
    {
        if (!OperatingSystem.IsWindows())
            return new VirtualTerminal(true, true, null, null);

        bool onOutput = TryEnable(Kernel32.StandardOutput, out uint? outputMode);
        bool onError = TryEnable(Kernel32.StandardError, out uint? errorMode);

        return new VirtualTerminal(onOutput, onError, outputMode, errorMode);
    }

    [SupportedOSPlatform("windows")]
    private static bool IsMissing(HANDLE* handle) => (nint)handle is 0 or Kernel32.InvalidHandleValue;

    [SupportedOSPlatform("windows")]
    private static void Restore(uint device, uint? mode)
    {
        if (mode is { } original)
            _ = unsafe (Kernel32.SetConsoleMode(Kernel32.GetStdHandle(device), original));
    }

    [SupportedOSPlatform("windows")]
    private static bool TryEnable(uint device, out uint? modeToRestore)
    {
        modeToRestore = null;
        HANDLE* handle = Kernel32.GetStdHandle(device);

        if (IsMissing(handle))
            return false;

        if (!unsafe (Kernel32.GetConsoleMode(handle, out uint mode)))
            return false;

        if ((mode & Kernel32.EnableVirtualTerminalProcessing) != 0)
            return true;

        if (!unsafe (Kernel32.SetConsoleMode(handle, mode | Kernel32.EnableVirtualTerminalProcessing)))
            return false;

        modeToRestore = mode;
        return true;
    }
}
