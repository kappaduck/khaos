// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Native;

internal sealed class SDLVersionsTests : IDisposable
{
    private const string VersionsPath = "sdl.env";

    private const string Versions = "# The versions native.yml builds.\nSDL3=3.4.20\nSDL3_IMAGE = 3.4.8\n# SDL3_MIXER=3.2.4\n";

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _path;

    public SDLVersionsTests() => _path = _workspace.Write(VersionsPath, Versions);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task GetVersionShouldReadTheVersionOfAKey()
    {
        SDLVersions versions = await SDLVersions.ParseAsync(_path);

        Version version = versions.GetVersion("SDL3");

        await version.Should().BeEqualTo(new Version(3, 4, 20));
    }

    [Test]
    public async Task GetVersionShouldIgnoreTheSpacesAroundTheEqualSign()
    {
        SDLVersions versions = await SDLVersions.ParseAsync(_path);

        Version version = versions.GetVersion("SDL3_IMAGE");

        await version.Should().BeEqualTo(new Version(3, 4, 8));
    }

    [Test]
    [Arguments("SDL3_TTF")]
    [Arguments("SDL3_MIXER")]
    public async Task GetVersionShouldThrowWhenTheKeyIsMissingOrCommented(string key)
    {
        SDLVersions versions = await SDLVersions.ParseAsync(_path);

        await Assert.That(() => versions.GetVersion(key))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'{_path}' has no {key} entry.");
    }

    [Test]
    public async Task GetVersionShouldThrowWhenTheValueIsNotAVersion()
    {
        string path = _workspace.Write(VersionsPath, "SDL3=main\n");

        SDLVersions versions = await SDLVersions.ParseAsync(path);

        await Assert.That(() => versions.GetVersion("SDL3"))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'main' is not a valid SDL3 version in '{path}'.");
    }

    [Test]
    public async Task SetVersionShouldThrowWhenTheKeyIsMissing()
    {
        SDLVersions versions = await SDLVersions.ParseAsync(_path);

        await Assert.That(() => versions.SetVersion("SDL3_TTF", new Version(3, 2, 6)))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'{_path}' has no SDL3_TTF entry.");
    }

    [Test]
    public async Task SaveAsyncShouldOnlyChangeTheVersionSet()
    {
        string path = _workspace.Write(VersionsPath, "SDL3=3.4.20\nSDL3_IMAGE = 3.4.8\nSDL3_MIXER=3.2.4\n");

        SDLVersions versions = await SDLVersions.ParseAsync(path);
        versions.SetVersion("SDL3", new Version(3, 4, 22));

        await versions.SaveAsync();

        await VerifyFile(_path);
    }
}
