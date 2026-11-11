// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Events;

namespace KappaDuck.Khaos.Analyzers.Tests.Events;

internal sealed class CustomEventsGeneratorTests
{
    private const string HintName = "Launcher.LauncherEvents.CustomEvents.g.cs";

    [Test]
    public async Task EditOfAnEventFieldShouldRecomputeTheGroup()
    {
        string[] sources = [CustomEventSources.Framework, CustomEventSources.Group];
        string[] edited = [CustomEventSources.Framework, CustomEventSources.Group.Replace("PlayerRenamed(string Name)", "PlayerRenamed(int Name)", StringComparison.Ordinal)];
        string[] recomputed = [CustomEventsGenerator.GroupsTracking];

        string failures = IncrementalChecks.Edit<CustomEventsGenerator>(sources, edited, [], recomputed);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task EditOfAnEventInAnotherFileShouldRecomputeTheGroup()
    {
        string[] sources = [CustomEventSources.Framework, CustomEventSources.Events, CustomEventSources.GroupOnly];
        string[] edited = [CustomEventSources.Framework, CustomEventSources.Events.Replace("PlayerRenamed(string Name)", "PlayerRenamed(int Name)", StringComparison.Ordinal), CustomEventSources.GroupOnly];
        string[] recomputed = [CustomEventsGenerator.GroupsTracking];

        string failures = IncrementalChecks.Edit<CustomEventsGenerator>(sources, edited, [], recomputed);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task EditOfAnUnrelatedFileShouldReuseEveryStep()
    {
        string[] sources = [CustomEventSources.Framework, CustomEventSources.Group, "public static class Score;"];
        string[] edited = [CustomEventSources.Framework, CustomEventSources.Group, "public static class Score { public const int Best = 3; }"];
        string[] reused = [CustomEventsGenerator.GroupsTracking, CustomEventsGenerator.SourcesTracking];

        string failures = IncrementalChecks.Edit<CustomEventsGenerator>(sources, edited, reused, []);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task PropertyPatternFromAChildNamespaceShouldCompileWithoutAUsing()
    {
        const string usage = """
            using KappaDuck.Khaos.Events;

            namespace Launcher.Scenes;

            public static class Usage
            {
                public static int Match(in Event e) => e switch
                {
                    CustomEvent { LauncherEvents: LevelCompleted done } => done.Level,
                    CustomEvent { LauncherEvents: DownloadFinished { Version: > 2 } } => 2,
                    _ => 0
                };
            }
            """;

        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, CustomEventSources.Group, usage);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task PropertyPatternFromAnotherNamespaceShouldCompileWithOneUsing()
    {
        const string usage = """
            using KappaDuck.Khaos.Events;
            using Launcher;

            namespace Shop;

            public static class Usage
            {
                public static int Match(in Event e) => e switch
                {
                    CustomEvent { LauncherEvents: LevelCompleted done } => done.Level,
                    CustomEvent { LauncherEvents: DownloadFinished { Version: > 2 } } => 2,
                    _ => 0
                };
            }
            """;

        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, CustomEventSources.Group, usage);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task RerunShouldReuseEveryStep()
    {
        string[] sources = [CustomEventSources.Framework, CustomEventSources.Group];

        string failures = IncrementalChecks.Rerun<CustomEventsGenerator>(sources, CustomEventsGenerator.GroupsTracking, CustomEventsGenerator.SourcesTracking);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldGenerateCodeThatCompiles()
    {
        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, CustomEventSources.Group);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldGenerateCodeThatCompilesWithEventsFromAnotherNamespace()
    {
        const string events = """
            namespace Launcher.Downloads;

            public readonly record struct DownloadFinished(int Version);
            """;

        const string group = """
            using KappaDuck.Khaos.Events;
            using Launcher.Downloads;

            namespace Launcher;

            [CustomEvents(typeof(DownloadFinished))]
            public readonly partial struct LauncherEvents;
            """;

        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, events, group);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldGenerateCodeThatCompilesWithoutTheMemorySafetyRules()
    {
        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(Parsing.WithoutMemorySafety, CustomEventSources.Framework, CustomEventSources.Group);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldLeaveOverlappedFieldsUnmarkedWithoutTheMemorySafetyRules()
    {
        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(Parsing.WithoutMemorySafety, CustomEventSources.Framework, CustomEventSources.Group);

        await result.Source(HintName).Should().NotContain(" safe ");
    }

    [Test]
    public async Task RunShouldMarkOverlappedFieldsSafeWithTheMemorySafetyRules()
    {
        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, CustomEventSources.Group);

        await result.Source(HintName).Should().Contain("internal readonly safe global::Launcher.DownloadFinished DownloadFinished;");
    }

    [Test]
    public async Task RunShouldNameTheFileAfterTheGroupWithoutANamespace()
    {
        const string group = """
            using KappaDuck.Khaos.Events;

            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            internal readonly partial struct ArcadeEvents;
            """;

        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, group);

        await result.Source("ArcadeEvents.CustomEvents.g.cs").Should().Contain("internal readonly partial struct ArcadeEvents");
    }

    [Test]
    public async Task RunShouldSkipAnInvalidGroupWithoutReportingIt()
    {
        const string group = """
            using KappaDuck.Khaos.Events;

            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            public readonly struct ArcadeEvents;
            """;

        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, group);

        await result.Ids.Should().BeEmpty();
        await result.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    public async Task RunShouldTagTheEventsInTheOrderListed()
    {
        const string first = "public LauncherEvents(global::Launcher.LevelCompleted value)\n    {\n        _tag = 1;";

        GeneratorResult result = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, CustomEventSources.Group);

        await result.Source(HintName).Should().Contain(first);
    }

    [Test]
    public Task VerifyAsyncShouldSnapshotTheGroup()
    {
        string[] sources = [CustomEventSources.Framework, CustomEventSources.Group];

        return GeneratorRunner<CustomEventsGenerator>.VerifyAsync(sources);
    }
}
