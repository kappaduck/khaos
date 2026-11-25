// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Native;

internal sealed class SDLUpdaterTests : IDisposable
{
    private const string Data = "Native/update";

    private static readonly Dictionary<string, Version?> _upToDate = Releases("3.4.20", "3.4.8", "3.2.2", "3.2.4");
    private static readonly Dictionary<string, Version?> _newSdl3 = Releases("3.4.22", null, "3.2.2", "3.2.4");
    private static readonly Dictionary<string, Version?> _newSdl3AndTtf = Releases("3.4.22", null, "3.2.6", "3.2.4");

    private static readonly SDLUpdateResult _result = new(
        [
            new SDLUpdate(new SDLLibrary("SDL3", "SDL", "SDL3"), new Version(3, 4, 20), new Version(3, 4, 22)),
            new SDLUpdate(new SDLLibrary("SDL3_TTF", "SDL_ttf", "SDL_ttf"), new Version(3, 2, 2), new Version(3, 2, 6)),
        ],
        new Version(1, 2, 1),
        new Version(1, 2, 2));

    private readonly Workspace _workspace = Workspace.Copy(Data);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task UpdateAsyncShouldReturnNoUpdateWhenEveryLibraryIsUpToDate()
    {
        SDLUpdateResult result = await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_upToDate[l.Key]));

        await result.Updated.Should().BeFalse();
    }

    [Test]
    public async Task UpdateAsyncShouldWriteNoFileWhenEveryLibraryIsUpToDate()
    {
        await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_upToDate[l.Key]));

        await VerifyDirectory(_workspace.Root);
    }

    [Test]
    public async Task UpdateAsyncShouldReturnTheLibrariesWithANewerRelease()
    {
        SDLUpdateResult result = await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await result.Updates.Count.Should().BeEqualTo(1);
        await result.Updates[0].Should().BeEqualTo(_result.Updates[0]);
    }

    [Test]
    public async Task UpdateAsyncShouldReturnTheNativeVersionBeforeAndAfterTheBump()
    {
        SDLUpdateResult result = await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await result.PreviousNative.Should().BeEqualTo(new Version(1, 2, 1));
        await result.NextNative.Should().BeEqualTo(new Version(1, 2, 2));
    }

    [Test]
    public async Task UpdateAsyncShouldWriteTheLatestVersionsInSdlEnv()
    {
        await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await VerifyFile(Path.Combine(_workspace.Root, SDLUpdater.VersionsPath));
    }

    [Test]
    public async Task UpdateAsyncShouldBumpTheVersionOfTheNativePackage()
    {
        await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await VerifyFile(Path.Combine(_workspace.Root, "native/packages/Native.nuspec"));
    }

    [Test]
    public async Task UpdateAsyncShouldUpdateTheBundledVersionsOfThePackageReadme()
    {
        await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await VerifyFile(Path.Combine(_workspace.Root, "native/packages/readme.md"));
    }

    [Test]
    public async Task UpdateAsyncShouldSetTheSourceRowOfTheCompatibilityTable()
    {
        await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await VerifyFile(Path.Combine(_workspace.Root, "readme.md"));
    }

    [Test]
    public async Task UpdateAsyncShouldAddTheNativeLineToTheChangelog()
    {
        await SDLUpdater.UpdateAsync(_workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await VerifyFile(Path.Combine(_workspace.Root, "changelog.md"));
    }

    [Test]
    public async Task UpdateAsyncShouldNameEveryLibraryBeforeTheFirstRelease()
    {
        using Workspace workspace = Workspace.Copy("Native/first-release");

        await SDLUpdater.UpdateAsync(workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));

        await VerifyFile(Path.Combine(workspace.Root, "changelog.md"));
    }

    [Test]
    public async Task UpdateAsyncShouldRewriteTheLineOfAnEarlierUpdate()
    {
        using Workspace workspace = Workspace.Copy("Native/before-fixed");

        await SDLUpdater.UpdateAsync(workspace.Root, l => Task.FromResult(_newSdl3[l.Key]));
        await SDLUpdater.UpdateAsync(workspace.Root, l => Task.FromResult(_newSdl3AndTtf[l.Key]));

        await VerifyFile(Path.Combine(workspace.Root, "changelog.md"));
    }

    [Test]
    public async Task UpdateAsyncShouldAddTheLineAtTheEndOfTheChangedList()
    {
        using Workspace workspace = Workspace.Copy("Native/changed-list");

        await SDLUpdater.UpdateAsync(workspace.Root, l => Task.FromResult(_newSdl3AndTtf[l.Key]));

        await VerifyFile(Path.Combine(workspace.Root, "changelog.md"));
    }

    [Test]
    public async Task PullRequestBodyShouldLinkEachReleaseAndNameTheBump()
    {
        string body = SDLUpdater.PullRequestBody(_result);

        await Verify(body);
    }

    [Test]
    public async Task StepSummaryShouldListEachUpdateWithoutLinks()
    {
        string summary = SDLUpdater.StepSummary(_result);

        await Verify(summary);
    }

    private static Dictionary<string, Version?> Releases(string sdl3, string? image, string ttf, string mixer) => new()
    {
        ["SDL3"] = Version.Parse(sdl3),
        ["SDL3_IMAGE"] = image is null ? null : Version.Parse(image),
        ["SDL3_TTF"] = Version.Parse(ttf),
        ["SDL3_MIXER"] = Version.Parse(mixer),
    };
}
