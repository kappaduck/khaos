// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Text.Json;

namespace KappaDuck.Khaos.Tools.Actions;

public static class Script
{
    private const int WrongUsage = 2;
    public const int Failed = 1;

    public static async Task<int> RunAsync(Func<Task<int>> body, string? afterError = null)
    {
        ArgumentNullException.ThrowIfNull(body);

        try
        {
            return await body();
        }
        catch (Exception ex) when (IsExpected(ex))
        {
            GitHubActions.Error(ex.Message);

            if (afterError is not null)
                await Console.Error.WriteLineAsync(afterError);

            return Failed;
        }
    }

    public static bool RunsFromRoot(params ReadOnlySpan<string> files)
    {
        foreach (string file in files)
        {
            if (File.Exists(file))
                continue;

            GitHubActions.Error($"Cannot find '{file}'. Run the script from the repository root.");
            return false;
        }

        return true;
    }

    public static int Usage(params ReadOnlySpan<string> lines)
    {
        for (int i = 0; i < lines.Length; i++)
            Console.Error.WriteLine(i == 0 ? $"usage: {lines[i]}" : $"       {lines[i]}");

        return WrongUsage;
    }

    private static bool IsExpected(Exception exception)
        => exception is ArgumentException or FormatException or HttpRequestException or InvalidOperationException or IOException or JsonException or KeyNotFoundException;
}
