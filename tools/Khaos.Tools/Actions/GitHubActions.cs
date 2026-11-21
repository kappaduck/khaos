// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Actions;

public static class GitHubActions
{
    private static readonly bool _inWorkflow = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

    public static void AppendStepSummary(string markdown)
    {
        string? file = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

        if (!string.IsNullOrEmpty(file))
            File.AppendAllText(file, markdown);
    }

    public static void Error(string message)
    {
        if (_inWorkflow)
        {
            Console.WriteLine($"::error::{message}");
            return;
        }

        Console.Error.WriteLine($"error: {message}");
    }

    public static void Notice(string message) => Console.WriteLine(_inWorkflow ? $"::notice::{message}" : message);

    public static void SetOutput(string name, string value)
    {
        string line = $"{name}={value}";
        string? file = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");

        if (string.IsNullOrEmpty(file))
        {
            Console.WriteLine(line);
            return;
        }

        File.AppendAllText(file, line + "\n");
    }

    public static void Warning(string message) => Console.WriteLine(_inWorkflow ? $"::warning::{message}" : $"warning: {message}");
}
