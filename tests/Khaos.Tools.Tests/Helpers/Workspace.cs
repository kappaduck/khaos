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

    internal static Workspace Copy(string path)
    {
        Workspace workspace = new();
        Folders.Copy(GetDataPath(path), workspace.Root);

        return workspace;
    }

    internal static Workspace Empty() => new();

    internal static string GetDataPath(string relative) => Path.Combine(AppContext.BaseDirectory, "Data", relative);

    internal static string ReadData(string relative) => ReadNormalized(GetDataPath(relative));

    internal string Read(string relative) => ReadNormalized(Path.Combine(Root, relative));

    internal string Write(string relative, string content)
    {
        string path = Path.Combine(Root, relative);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);

        return path;
    }

    private static string ReadNormalized(string path) => File.ReadAllText(path).ReplaceLineEndings("\n");
}
