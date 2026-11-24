// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Native;

internal sealed class NativePackageTests : IDisposable
{
    private const string NuspecPath = "Native.nuspec";

    private const string Nuspec = """
        <?xml version="1.0" encoding="utf-8"?>
        <package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
          <metadata>
            <id>KappaDuck.Khaos.Native</id>
            <version>1.2.1</version>
          </metadata>
        </package>
        """;

    private static readonly SDLLibrary _sdl3 = new("SDL3", "SDL", "SDL3");
    private static readonly SDLLibrary _ttf = new("SDL3_TTF", "SDL_ttf", "SDL_ttf");

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _path;

    public NativePackageTests() => _path = _workspace.Write(NuspecPath, Nuspec);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task ParseAsyncShouldReadTheVersion()
    {
        NativePackage package = await NativePackage.ParseAsync(_path);

        await package.Version.Should().BeEqualTo(new Version(1, 2, 1));
    }

    [Test]
    public async Task ParseAsyncShouldThrowWhenTheVersionIsMissing()
    {
        string path = _workspace.Write(NuspecPath, "<package />");

        await Assert.That(async () => await NativePackage.ParseAsync(path))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"Cannot find the package version in '{path}'.");
    }

    [Test]
    [Arguments("1.2")]
    [Arguments("next")]
    public async Task ParseAsyncShouldThrowWhenTheVersionIsNotMajorMinorPatch(string version)
    {
        string path = _workspace.Write(NuspecPath, Nuspec.Replace("1.2.1", version, StringComparison.Ordinal));

        await Assert.That(async () => await NativePackage.ParseAsync(path))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'{version}' in '{path}' is not a major.minor.patch version.");
    }

    [Test]
    [Arguments("3.4.20", "3.4.22", "1.2.2")]
    [Arguments("3.4.20", "3.6.0", "1.3.0")]
    [Arguments("3.4.20", "4.0.0", "2.0.0")]
    public async Task BumpShouldFollowTheLevelOfTheSdlChange(string current, string latest, string expected)
    {
        NativePackage package = await NativePackage.ParseAsync(_path);

        package.Bump([new SDLUpdate(_sdl3, Version.Parse(current), Version.Parse(latest))]);

        await package.Version.Should().BeEqualTo(Version.Parse(expected));
    }

    [Test]
    public async Task BumpShouldTakeTheHighestLevelOfTheUpdates()
    {
        SDLUpdate patch = new(_sdl3, new Version(3, 4, 20), new Version(3, 4, 22));
        SDLUpdate minor = new(_ttf, new Version(3, 2, 2), new Version(3, 4, 0));

        NativePackage package = await NativePackage.ParseAsync(_path);
        package.Bump([patch, minor]);

        await package.Version.Should().BeEqualTo(new Version(1, 3, 0));
    }

    [Test]
    public async Task SaveAsyncShouldOnlyChangeTheVersion()
    {
        NativePackage package = await NativePackage.ParseAsync(_path);
        package.Bump([new SDLUpdate(_sdl3, new Version(3, 4, 20), new Version(3, 4, 22))]);

        await package.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task SaveAsyncShouldWriteAVersionLongerThanTheOneRead()
    {
        string path = _workspace.Write(NuspecPath, Nuspec.Replace("1.2.1", "1.2.9", StringComparison.Ordinal));

        NativePackage package = await NativePackage.ParseAsync(path);
        package.Bump([new SDLUpdate(_sdl3, new Version(3, 4, 20), new Version(3, 4, 22))]);

        await package.SaveAsync();

        await VerifyFile(path);
    }
}
