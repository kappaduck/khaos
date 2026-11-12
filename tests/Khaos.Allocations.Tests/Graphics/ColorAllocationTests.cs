// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;
using System.Globalization;

namespace KappaDuck.Khaos.Allocations.Tests.Graphics;

internal sealed class ColorAllocationTests
{
    [Test]
    [Arguments("#6495ED")]
    [Arguments("6495ED80")]
    [Arguments("#ABC")]
    public async Task TryFromHexShouldNotAllocate(string hex)
    {
        long allocated = Allocated.By(hex, static text => Color.TryFromHex(text, out _));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryFormatShouldNotAllocate()
    {
        char[] buffer = new char[32];

        long allocated = Allocated.By(buffer, static destination =>
            Color.CornflowerBlue.TryFormat(destination, out _, default, CultureInfo.InvariantCulture));

        await allocated.Should().BeEqualTo(0L);
    }
}
