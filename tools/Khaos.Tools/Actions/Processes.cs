// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics;

namespace KappaDuck.Khaos.Tools.Actions;

public static class Processes
{
    public static Task<ProcessOutput> GitAsync(params IEnumerable<string> arguments) => ReadAsync("git", arguments);

    public static Task<int> RunAsync(string fileName, params IEnumerable<string> arguments)
        => RunAsync(new Dictionary<string, string>(), fileName, arguments);

    internal static async Task<int> RunAsync(IReadOnlyDictionary<string, string> environment, string fileName, params IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(environment);

        string[] list = [.. arguments];
        Console.WriteLine($"> {fileName} {string.Join(' ', list)}");

        ProcessStartInfo info = new(fileName, list);

        foreach ((string name, string value) in environment)
            info.Environment[name] = value;

        StopWithTool(info);

        ProcessExitStatus status = await Process.RunAsync(info);
        return status.ExitCode;
    }

    private static async Task<ProcessOutput> ReadAsync(string fileName, params IEnumerable<string> arguments)
    {
        ProcessStartInfo info = new(fileName, arguments) { RedirectStandardOutput = true, RedirectStandardError = true };
        StopWithTool(info);

        ProcessTextOutput result = await Process.RunAndCaptureTextAsync(info);
        await Console.Error.WriteAsync(result.StandardError);

        return new ProcessOutput(result.ExitStatus.ExitCode, result.StandardOutput);
    }

    private static void StopWithTool(ProcessStartInfo info)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
            info.KillOnParentExit = true;
    }
}
