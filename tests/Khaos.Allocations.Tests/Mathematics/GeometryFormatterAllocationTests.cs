// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Allocations.Tests.Helpers;
using KappaDuck.Khaos.Mathematics;
using System.Globalization;

namespace KappaDuck.Khaos.Allocations.Tests.Mathematics;

internal sealed class GeometryFormatterAllocationTests
{
    private static readonly float[] _components = [1.5f, 2.5f, 3.5f, 4.5f];
    private static readonly float[] _thousands = [1000f, 2000f];

    [Test]
    [Arguments("")]
    [Arguments("fr-CA")]
    public async Task TryFormatShouldNotAllocate(string culture)
    {
        char[] buffer = new char[64];
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        long allocated = Allocated.By((buffer, provider), static state => GeometryFormatter.TryFormat(state.buffer, out _, _components, default, state.provider));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    [Arguments("")]
    [Arguments("fr-CA")]
    public async Task TryFormatUtf8ShouldNotAllocate(string culture)
    {
        byte[] buffer = new byte[64];
        CultureInfo provider = CultureInfo.GetCultureInfo(culture);

        long allocated = Allocated.By((buffer, provider), static state => GeometryFormatter.TryFormat(state.buffer, out _, _components, default, state.provider));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task TryFormatShouldNotAllocateWhenAComponentContainsAComma()
    {
        char[] buffer = new char[64];

        long allocated = Allocated.By(buffer, static destination => GeometryFormatter.TryFormat(destination, out _, _thousands, "N0", CultureInfo.InvariantCulture));

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task ToStringShouldAllocateOnlyTheString()
    {
        Point point = new(1.5f, 2.5f);
        string expected = GeometryFormatter.ToString(point, null, CultureInfo.InvariantCulture);
        long stringSize = Allocated.By(expected, static text => new string(text.AsSpan()));

        long allocated = Allocated.By(point, static value => GeometryFormatter.ToString(value, null, CultureInfo.InvariantCulture));

        await allocated.Should().BeEqualTo(stringSize);
    }
}
