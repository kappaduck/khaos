// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Packages;

namespace KappaDuck.Khaos.Tools.Tests.Packages;

internal sealed class UsageSamplesTests
{
    [Test]
    public async Task ParseShouldReadEachBlockWithItsPath()
    {
        const string readme = "# Pong\n\n## Usage\n\n```csharp\n// Pong/PongGame.cs\nclass PongGame;\n```\n\n```csharp\n// Pong/Program.cs\nnew PongGame();\n```\n\n## License\n";

        IReadOnlyList<UsageSample>? samples = UsageSamples.Parse(readme)!;

        await samples[0].Path.Should().BeEqualTo("Pong/PongGame.cs");
        await samples[0].Code.Should().BeEqualTo("// Pong/PongGame.cs\nclass PongGame;\n");

        await samples[1].Path.Should().BeEqualTo("Pong/Program.cs");
        await samples[1].Code.Should().BeEqualTo("// Pong/Program.cs\nnew PongGame();\n");
    }

    [Test]
    [Arguments("# Pong\n\nNo usage.\n")]
    [Arguments("# Pong\n\n## Usage\n\nNo block.\n")]
    [Arguments("# Pong\n\n## Usage\n\n```csharp\nclass PongGame;\n```\n")]
    public async Task ParseShouldRefuseAReadmeWithoutSamplesThatSayWhereTheyGo(string readme)
    {
        IReadOnlyList<UsageSample>? samples = UsageSamples.Parse(readme);

        await samples.Should().BeNull();
    }
}
