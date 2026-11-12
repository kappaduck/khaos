// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Hosting;

namespace KappaDuck.Khaos.Analyzers.Tests.Hosting;

internal sealed class RedundantWindowAnalyzerTests
{
    private const string Usings = """
        using KappaDuck.Khaos;
        using KappaDuck.Khaos.Windowing;

        namespace Arcade;


        """;

    private const string DerivedGame = Usings + """
        public abstract class ArcadeGame(int width, int height) : Game2D(width, height);

        public sealed class PinballGame : ArcadeGame
        {
            public PinballGame(Window window) : base(800, 600)
            {
            }
        }
        """;

    private const string GameWithoutWindow = Usings + """
        public sealed class HighScores;

        public sealed class PinballGame(HighScores scores) : Game2D(800, 600)
        {
            public HighScores Scores { get; } = scores;
        }
        """;

    private const string PrimaryConstructor = Usings + """
        public sealed class PinballGame(Window window) : Game2D(800, 600)
        {
            protected override void Initialize() => window.Title = "Pinball";
        }
        """;

    private const string RegularConstructor = Usings + """
        public sealed class PinballGame : Game2D
        {
            public PinballGame(Window window) : base(800, 600)
            {
            }
        }
        """;

    private const string Service = Usings + """
        public sealed class Camera(Window window)
        {
            public Window Window { get; } = window;
        }
        """;

    [Test]
    public async Task DerivedGameShouldWarnKHAOS006()
    {
        AnalyzerResult result = await AnalyzerRunner<RedundantWindowAnalyzer>.RunAsync(HostingSources.Framework, DerivedGame);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHAOS006");
    }

    [Test]
    public async Task GameWithoutWindowShouldReportNothing()
    {
        AnalyzerResult result = await AnalyzerRunner<RedundantWindowAnalyzer>.RunAsync(HostingSources.Framework, GameWithoutWindow);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task PrimaryConstructorShouldWarnKHAOS006()
    {
        AnalyzerResult result = await AnalyzerRunner<RedundantWindowAnalyzer>.RunAsync(HostingSources.Framework, PrimaryConstructor);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(6,40): warning KHAOS006: 'PinballGame' takes the window as 'window', but Game2D already gives it as Window; use Window from Initialize on");
    }

    [Test]
    public async Task RegularConstructorShouldWarnKHAOS006()
    {
        AnalyzerResult result = await AnalyzerRunner<RedundantWindowAnalyzer>.RunAsync(HostingSources.Framework, RegularConstructor);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(8,31): warning KHAOS006: 'PinballGame' takes the window as 'window', but Game2D already gives it as Window; use Window from Initialize on");
    }

    [Test]
    public async Task ServiceShouldReportNothing()
    {
        AnalyzerResult result = await AnalyzerRunner<RedundantWindowAnalyzer>.RunAsync(HostingSources.Framework, Service);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
