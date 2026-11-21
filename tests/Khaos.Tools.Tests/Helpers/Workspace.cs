// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tools.Tests.Helpers;

internal sealed class Workspace : IDisposable
{
    private readonly TempDirectory _temp = new();

    private Workspace()
    {
    }

    internal string Root => _temp.Path;

    public void Dispose() => _temp.Dispose();

    public static Workspace Copy(string data)
    {
        Workspace workspace = new();
        Folders.Copy(DataPath(data), workspace.Root);

        return workspace;

        static string DataPath(string relative) => Path.Combine(AppContext.BaseDirectory, "Data", relative);
    }

    internal static Workspace Empty() => new();

    internal string Write(string relative, string content)
    {
        string path = Path.Combine(Root, relative);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);

        return path;
    }
}
