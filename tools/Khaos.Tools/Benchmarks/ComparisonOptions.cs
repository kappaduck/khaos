// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace KappaDuck.Khaos.Tools.Benchmarks;

public sealed record ComparisonOptions(string Base, string Diff, string BaseName, string DiffName, string Threshold, bool Strict)
{
    public static bool TryParse(ReadOnlySpan<string> args, [NotNullWhen(true)] out ComparisonOptions? options)
    {
        options = null;

        string? baseFolder = null;
        string? diffFolder = null;
        string baseName = "base";
        string diffName = "diff";
        string threshold = "5%";
        bool strict = false;

        int index = 0;

        while (index < args.Length)
        {
            string name = args[index++];

            if (name == "--strict")
            {
                strict = true;
                continue;
            }

            if (index == args.Length)
                return false;

            string value = args[index++];

            switch (name)
            {
                case "--base":
                    baseFolder = value;
                    break;

                case "--diff":
                    diffFolder = value;
                    break;

                case "--base-name":
                    baseName = value;
                    break;

                case "--diff-name":
                    diffName = value;
                    break;

                case "--threshold":
                    threshold = value;
                    break;

                default:
                    return false;
            }
        }

        if (baseFolder is null || diffFolder is null)
            return false;

        options = new ComparisonOptions(baseFolder, diffFolder, baseName, diffName, threshold, strict);
        return true;
    }
}
