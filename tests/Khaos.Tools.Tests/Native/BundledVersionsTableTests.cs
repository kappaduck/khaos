// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Native;

internal sealed class BundledVersionsTableTests : IDisposable
{
    private const string ReadmePath = "readme.md";

    private const string Readme = """
        # KappaDuck.Khaos.Native

        ## Bundled versions

        | Library   | Version  |
        | :-------- | :------: |
        | SDL3      | `3.4.20` |
        | SDL_image | `3.4.8`  |

        The same version on every platform.
        """;

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _path;

    public BundledVersionsTableTests() => _path = _workspace.Write(ReadmePath, Readme);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task ParseAsyncShouldThrowWhenTheTableIsMissing()
    {
        string path = _workspace.Write(ReadmePath, "# KappaDuck.Khaos.Native\n");

        await Assert.That(async () => await BundledVersionsTable.ParseAsync(path))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"Cannot find the table under '## Bundled versions' in '{path}'.");
    }

    [Test]
    public async Task UpdateShouldWriteTheVersionOfEachLibrary()
    {
        Dictionary<string, Version> bundled = new() { ["SDL3"] = new Version(3, 4, 22), ["SDL_image"] = new Version(3, 4, 10) };

        BundledVersionsTable table = await BundledVersionsTable.ParseAsync(_path);
        table.Update(bundled);

        await table.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task UpdateShouldKeepTheVersionOfALibraryNotGiven()
    {
        Dictionary<string, Version> bundled = new() { ["SDL3"] = new Version(3, 4, 22) };

        BundledVersionsTable table = await BundledVersionsTable.ParseAsync(_path);
        table.Update(bundled);

        await table.SaveAsync();

        await VerifyFile(_path);
    }
}
