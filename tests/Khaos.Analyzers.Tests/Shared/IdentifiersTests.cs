// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared;

internal sealed class IdentifiersTests
{
    [Test]
    public async Task EscapeShouldAddAtSymbolWhenIsKeyword()
    {
        string identifier = Identifiers.Escape("event");

        await identifier.Should().BeEqualTo("@event");
    }

    [Test]
    public async Task EscapeShouldNotAddAtSymbolWhenIsNotKeyword()
    {
        string identifier = Identifiers.Escape("khaos");

        await identifier.Should().BeEqualTo("khaos");
    }
}
