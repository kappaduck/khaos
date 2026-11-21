// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Releases;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Releases;

internal sealed class VersionPrefixTests : IDisposable
{
    private const string PropsPath = "khaos.props";

    private const string Content = """
        <Project>
          <PropertyGroup>
            <VersionPrefix>0.1.0</VersionPrefix>
          </PropertyGroup>
        </Project>
        """;

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _path;

    public VersionPrefixTests() => _path = _workspace.Write(PropsPath, Content);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task ParseAsyncShouldGetTheVersion()
    {
        VersionPrefix prefix = await VersionPrefix.ParseAsync(_path);

        await prefix.Version.Should().BeEqualTo(new Version(0, 1, 0));
    }

    [Test]
    public async Task ParseAsyncShouldThrowsWhenOpenTagIsMissing()
    {
        const string missingTag = "<Project></Project>";

        string path = _workspace.Write(PropsPath, missingTag);

        await Assert.That(() => VersionPrefix.ParseAsync(path)!)
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"Cannot find a <VersionPrefix> element in '{path}'.");
    }

    [Test]
    [Arguments("a.b.c")]
    [Arguments("1.0")]
    [Arguments("x.2.0")]
    [Arguments("1.2.2025.0")]
    [Arguments("12")]
    public async Task ParseAsyncShouldThrowsWhenVersionIsNotValid(string version)
    {
        string badVersion = $"""
            <Project>
              <PropertyGroup>
                <VersionPrefix>{version}</VersionPrefix>
              </PropertyGroup>
            </Project>
            """;

        string path = _workspace.Write(PropsPath, badVersion);

        await Assert.That(() => VersionPrefix.ParseAsync(path)!)
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'{version}' is not a major.minor.patch VersionPrefix in '{path}'.");
    }

    [Test]
    public async Task RaiseShouldRaiseTheNewVersion()
    {
        Version next = new(0, 2, 0);

        VersionPrefix prefix = await VersionPrefix.ParseAsync(_path);
        prefix.Raise(next);

        await prefix.Version.Should().BeEqualTo(next);
    }

    [Test]
    public async Task RaiseShouldThrowsWhenVersionIsLessThanTheParsedVersion()
    {
        Version next = new(0, 0, 1);

        VersionPrefix prefix = await VersionPrefix.ParseAsync(_path);

        await Assert.That(() => prefix.Raise(next))
                    .ThrowsExactly<ArgumentException>()
                    .WithMessage($"The next version ({next}) must be greater than the current one (0.1.0) (Parameter 'version')");
    }

    [Test]
    public async Task RaiseShouldSaveTheContentWithNewVersion()
    {
        Version next = new(0, 2, 0);

        VersionPrefix prefix = await VersionPrefix.ParseAsync(_path);
        prefix.Raise(next);

        await prefix.SaveAsync();

        await VerifyFile(_path);
    }
}
