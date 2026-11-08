// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using KappaDuck.Khaos.Tests.Helpers;

namespace KappaDuck.Khaos.Tests.Hosting;

[NotInParallel(Sequential.Engine)]
internal sealed class AppMetadataTests
{
    [After(Test)]
    public void ClearAppContext() => GameMetadata.Clear();

    [Test]
    public async Task FromAppContextShouldReadTheName()
    {
        AppContext.SetData(GameMetadata.Name, "Duck Quest");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Name.Should().BeEqualTo("Duck Quest");
    }

    [Test]
    public async Task FromAppContextShouldReadTheVersion()
    {
        AppContext.SetData(GameMetadata.Version, "1.2.0");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Version.Should().BeEqualTo("1.2.0");
    }

    [Test]
    public async Task FromAppContextShouldReadTheCreator()
    {
        AppContext.SetData(GameMetadata.Creator, "KappaDuck");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Creator.Should().BeEqualTo("KappaDuck");
    }

    [Test]
    public async Task FromAppContextShouldReadTheCopyright()
    {
        AppContext.SetData(GameMetadata.Copyright, "Copyright (c) 2027 KappaDuck");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Copyright.Should().BeEqualTo("Copyright (c) 2027 KappaDuck");
    }

    [Test]
    public async Task FromAppContextShouldReadTheIdentifier()
    {
        AppContext.SetData(GameMetadata.Identifier, "com.kappaduck.duckquest");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Identifier.Should().BeEqualTo("com.kappaduck.duckquest");
    }

    [Test]
    public async Task FromAppContextShouldReadTheUrl()
    {
        AppContext.SetData(GameMetadata.Url, "https://kappaduck.itch.io/duck-quest");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Url.Should().BeEqualTo(new Uri("https://kappaduck.itch.io/duck-quest"));
    }

    [Test]
    public async Task FromAppContextShouldLeaveTheNameNullWhenItIsNotSet()
    {
        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Name.Should().BeNull();
    }

    [Test]
    public async Task FromAppContextShouldLeaveTheNameNullWhenItIsEmpty()
    {
        AppContext.SetData(GameMetadata.Name, string.Empty);

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Name.Should().BeNull();
    }

    [Test]
    public async Task FromAppContextShouldLeaveTheUrlNullWhenItIsNotAbsolute()
    {
        AppContext.SetData(GameMetadata.Url, "duck-quest");

        AppMetadata metadata = AppMetadata.FromAppContext();

        await metadata.Url.Should().BeNull();
    }
}
