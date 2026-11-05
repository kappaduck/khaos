// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Graphics;

namespace KappaDuck.Khaos.Tests.Graphics;

internal sealed class ColorTests
{
    [Test]
    [MethodDataSource(nameof(NamedColors))]
    public async Task NamedColorShouldMatchItsCssValue(Color named, Color expected)
    {
        await named.Should().BeEqualTo(expected);
    }

    public static IEnumerable<Func<(Color Named, Color Expected)>> NamedColors()
    {
        yield return static () => (Color.Black, new Color(0, 0, 0));
        yield return static () => (Color.White, new Color(255, 255, 255));
        yield return static () => (Color.CornflowerBlue, new Color(100, 149, 237));
        yield return static () => (Color.RebeccaPurple, new Color(102, 51, 153));
        yield return static () => (Color.LightGoldenrodYellow, new Color(250, 250, 210));
    }
}
