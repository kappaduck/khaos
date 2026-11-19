// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Markdown;

internal sealed class ChangelogTests : IDisposable
{
    private const string ChangelogPath = "changelog.md";

    private const string Released = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-11-19

        ### Added

        - The first version.
        """;

    private const string Unreleased = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]

        ### Added

        - The first version.
        """;

    private static readonly Version _first = new(0, 1, 0);
    private static readonly Version _second = new(0, 2, 0);
    private static readonly Version _native = new(1, 2, 1);

    private static readonly DateOnly _releaseDay = new(2026, 11, 19);

    private readonly Workspace _workspace = Workspace.Empty();

    private readonly string _path;

    public ChangelogTests() => _path = _workspace.Write(ChangelogPath, Unreleased);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task ReleaseShouldDatesTheUnreleasedSection()
    {
        Changelog changelog = await Changelog.ParseAsync(_path);

        changelog.Release(_first, _releaseDay);
        await changelog.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task ReleaseShouldThrowsWhenThereIsNoUnreleasedSection()
    {
        string path = _workspace.Write(ChangelogPath, Released);

        Changelog changelog = await Changelog.ParseAsync(path);

        await Assert.That(() => changelog.Release(_first, _releaseDay))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"'{path}' has no [Unreleased] section.");
    }

    [Test]
    public async Task ReleaseShouldThrowsWhenThereIsAlreadyASection()
    {
        const string secondRelease = """
        # Khaos changelog

        ## 0.2.0 &#8212; 2026-xx-xx [Unreleased]

        ## 0.1.0 &#8212; 2026-11-19

        ### Added

        - The first version.
        """;

        string path = _workspace.Write(ChangelogPath, secondRelease);

        Changelog changelog = await Changelog.ParseAsync(path);

        await Assert.That(() => changelog.Release(_first, _releaseDay))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"'{path}' already has a section for 0.1.0.");
    }

    [Test]
    public async Task ReleaseShouldThrowsWhenTheUnreleasedSectionIsEmpty()
    {
        const string emptyReleased = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]
        """;

        string path = _workspace.Write(ChangelogPath, emptyReleased);

        Changelog changelog = await Changelog.ParseAsync(path);

        await Assert.That(() => changelog.Release(_first, _releaseDay))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"The [Unreleased] section of '{path}' is empty.");
    }

    [Test]
    public async Task OpenNextShouldThrowsWhenUnreleasedSectionExistsForTheNextVersion()
    {
        const string secondRelease = """
        # Khaos changelog

        ## 0.2.0 &#8212; 2026-xx-xx [Unreleased]

        ## 0.1.0 &#8212; 2026-11-19

        ### Added

        - The first version.
        """;

        string path = _workspace.Write(ChangelogPath, secondRelease);

        Changelog changelog = await Changelog.ParseAsync(path);

        await Assert.That(() => changelog.OpenNext(_first, _second, 2026))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"'{_path}' already has an [Unreleased] section.");
    }

    [Test]
    public async Task OpenNextShouldThrowsWhenThereIsNoCurrentRelease()
    {
        const string missingRelease = """
        # Khaos changelog

        ## 0.0.1 &#8212; 2026-11-19

        ### Added

        - The first version.
        """;

        string path = _workspace.Write(ChangelogPath, missingRelease);

        Changelog changelog = await Changelog.ParseAsync(path);

        await Assert.That(() => changelog.OpenNext(_first, _second, 2026))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"'{_path}' has no section for 0.1.0. Prepare and publish the release first.");
    }

    [Test]
    public async Task OpenNextShouldAddNewUnreleasedSection()
    {
        string path = _workspace.Write(ChangelogPath, Released);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.OpenNext(_first, _second, 2026);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }

    [Test]
    public async Task SetNativeShouldThrowsWhenThereIsNoUnreleasedSection()
    {
        string path = _workspace.Write(ChangelogPath, Released);

        Changelog changelog = await Changelog.ParseAsync(path);

        await Assert.That(() => changelog.SetNative(_native, []))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"'{_path}' has no [Unreleased] section. Open it with 'dotnet tools/release/next.cs' first.");
    }

    [Test]
    public async Task SetNativeShouldSetNativeInChangedSection()
    {
        const string Native = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]
        """;

        string path = _workspace.Write(ChangelogPath, Native);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.SetNative(_native, []);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }

    [Test]
    public async Task SetNativeShouldReplaceNativeLineWhenIsAlreadyThere()
    {
        const string Native = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]

        ### Changed

        - `KappaDuck.Khaos.Native` 1.2.0
        """;

        string path = _workspace.Write(ChangelogPath, Native);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.SetNative(_native, []);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }

    [Test]
    public async Task SetNativeShouldAddToChangedSectionWhenAlreadyHaveChangedSection()
    {
        const string Native = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]

        ### Changed

        - Modify something!
        """;

        string path = _workspace.Write(ChangelogPath, Native);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.SetNative(_native, []);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }

    [Test]
    public async Task SetNativeShouldAddNativeLineToChangedSectionNoMatterTheOrderOfChangelogSection()
    {
        const string Native = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]

        ### Changed

        - Modify something!

        ### Added

        - Added new way to set the native line

        ### Fixed

        - Found a bug!
        """;

        string path = _workspace.Write(ChangelogPath, Native);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.SetNative(_native, []);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }

    [Test]
    public async Task SetNativeShouldAddChangedSectionWithNativeLineNoMatterTheOrderOfChangelogSection()
    {
        const string Native = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]

        ### Added

        - Added new way to set the native line

        ### Fixed

        - Found a bug!
        """;

        string path = _workspace.Write(ChangelogPath, Native);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.SetNative(_native, []);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }

    [Test]
    public async Task SetNativeShouldSetNativeWithHisLibrariesInChangedSection()
    {
        const string Native = """
        # Khaos changelog

        ## 0.1.0 &#8212; 2026-xx-xx [Unreleased]
        """;

        KeyValuePair<string, Version>[] sdl =
        [
            new KeyValuePair<string, Version>("SDL3", new(3, 4, 20)),
            new KeyValuePair<string, Version>("SDL3_image", new(3, 4, 8)),
            new KeyValuePair<string, Version>("SDL3_ttf", new(3, 2, 2)),
            new KeyValuePair<string, Version>("SDL3_mixer", new(3, 2, 4))
        ];

        string path = _workspace.Write(ChangelogPath, Native);

        Changelog changelog = await Changelog.ParseAsync(path);

        changelog.SetNative(_native, sdl);
        await changelog.SaveAsync();

        await VerifyFile(path);
    }
}
