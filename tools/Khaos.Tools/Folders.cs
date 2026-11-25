// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools;

internal static class Folders
{
    internal static void Copy(string source, string destination, bool overwrite = false)
    {
        Directory.CreateDirectory(destination);

        foreach (string folder in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, folder)));

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)), overwrite);
    }

    internal static string[] Sorted(string[] folders)
    {
        Array.Sort(folders, StringComparer.Ordinal);
        return folders;
    }
}
