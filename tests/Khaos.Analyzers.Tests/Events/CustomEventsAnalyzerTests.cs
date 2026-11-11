// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Events;

namespace KappaDuck.Khaos.Analyzers.Tests.Events;

internal sealed class CustomEventsAnalyzerTests
{
    private const string Usings = """
        using System.Collections.Generic;
        using KappaDuck.Khaos.Events;

        namespace Arcade;


        """;

    private const string ClassEvent = Usings + """
        public sealed class CoinInserted;

        [CustomEvents(typeof(CoinInserted))]
        public readonly partial struct ArcadeEvents;
        """;

    private const string DuplicateEvent = Usings + """
        public readonly record struct CoinInserted(int Credits);

        [CustomEvents(typeof(CoinInserted), typeof(CoinInserted))]
        public readonly partial struct ArcadeEvents;
        """;

    private const string EmptyGroup = Usings + """
        [CustomEvents]
        public readonly partial struct ArcadeEvents;
        """;

    private const string LessAccessibleEvent = Usings + """
        internal readonly record struct CoinInserted(int Credits);

        [CustomEvents(typeof(CoinInserted))]
        public readonly partial struct ArcadeEvents;
        """;

    private const string ListField = Usings + """
        public readonly record struct HighScores(List<int> Scores);

        [CustomEvents(typeof(HighScores))]
        public readonly partial struct ArcadeEvents;
        """;

    private const string MissingPartial = Usings + """
        public readonly record struct CoinInserted(int Credits);

        [CustomEvents(typeof(CoinInserted))]
        public readonly struct ArcadeEvents;
        """;

    private const string NestedGroup = Usings + """
        public readonly record struct CoinInserted(int Credits);

        public static class Cabinet
        {
            [CustomEvents(typeof(CoinInserted))]
            public readonly partial struct ArcadeEvents;
        }
        """;

    private const string RecordGroup = Usings + """
        public readonly record struct CoinInserted(int Credits);

        [CustomEvents(typeof(CoinInserted))]
        public readonly partial record struct ArcadeEvents;
        """;

    private const string ReservedName = Usings + """
        public readonly record struct Cases(int Credits);

        [CustomEvents(typeof(Cases))]
        public readonly partial struct ArcadeEvents;
        """;

    private const string SameNameEvents = """
        using KappaDuck.Khaos.Events;

        namespace Arcade.Menu
        {
            public readonly record struct Started(int Credits);
        }

        namespace Arcade.Game
        {
            public readonly record struct Started(int Level);
        }

        namespace Arcade
        {
            [CustomEvents(typeof(Menu.Started), typeof(Game.Started))]
            public readonly partial struct ArcadeEvents;
        }
        """;

    [Test]
    public async Task ClassEventShouldReportKHAOS003()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, ClassEvent);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(8,15): error KHAOS003: 'CoinInserted' in 'ArcadeEvents' is not a non-generic readonly record struct");
    }

    [Test]
    public async Task DuplicateEventShouldReportKHAOS005()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, DuplicateEvent);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(8,37): error KHAOS005: 'Arcade.CoinInserted' in 'ArcadeEvents' is listed twice; list it once");
    }

    [Test]
    public async Task EmptyGroupShouldReportKHAOS002()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, EmptyGroup);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHAOS002");
    }

    [Test]
    public async Task EventsWithTheSameNameShouldReportKHAOS005()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, SameNameEvents);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(15,41): error KHAOS005: 'Arcade.Game.Started' in 'ArcadeEvents' has the same name as 'Arcade.Menu.Started'; rename one of them");
    }

    [Test]
    public async Task GroupNamedLikeAMemberOfCustomEventShouldWarnKHAOS004()
    {
        const string source = Usings + """
            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            public readonly partial struct TryGet;
            """;

        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, source);

        await result.Reported.Should().BeEqualTo("Source1.cs(9,32): warning KHAOS004: 'TryGet' has the name of a member of CustomEvent, so 'CustomEvent { TryGet: … }' reads that member instead of the group; rename the group");
    }

    [Test]
    [Arguments(ClassEvent)]
    [Arguments(DuplicateEvent)]
    [Arguments(EmptyGroup)]
    [Arguments(LessAccessibleEvent)]
    [Arguments(ListField)]
    [Arguments(MissingPartial)]
    [Arguments(NestedGroup)]
    [Arguments(RecordGroup)]
    [Arguments(ReservedName)]
    [Arguments(SameNameEvents)]
    public async Task InvalidGroupShouldBeReportedAndNotGenerated(string invalid)
    {
        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, invalid);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(CustomEventSources.Framework, invalid);

        await reported.Ids.Should().NotBeEmpty();
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    public async Task LessAccessibleEventShouldReportKHAOS003()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, LessAccessibleEvent);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(8,15): error KHAOS003: 'CoinInserted' in 'ArcadeEvents' is less accessible than the group; make it public, or make the group internal");
    }

    [Test]
    public async Task ListFieldShouldReportKHAOS003()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, ListField);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(8,15): error KHAOS003: 'HighScores' in 'ArcadeEvents' holds 'Scores' of type System.Collections.Generic.List<int>; an event holds only values and strings");
    }

    [Test]
    public async Task MissingPartialShouldReportKHAOS001()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, MissingPartial);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(9,24): error KHAOS001: 'ArcadeEvents' is marked [CustomEvents]; declare it as a readonly partial struct, not generic and not nested in another type");
    }

    [Test]
    public async Task NestedGroupShouldReportKHAOS001()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, NestedGroup);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHAOS001");
    }

    [Test]
    public async Task RecordGroupShouldReportKHAOS001()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, RecordGroup);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHAOS001");
    }

    [Test]
    public async Task ReservedNameShouldReportKHAOS003()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, ReservedName);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(8,15): error KHAOS003: 'Cases' in 'ArcadeEvents' has a name that the group uses for its own members; rename the event");
    }

    [Test]
    public async Task ValidGroupShouldReportNothing()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, CustomEventSources.Group);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public Task VerifyAsyncShouldSnapshotTheDiagnostics()
    {
        string[] sources = [CustomEventSources.Framework, MissingPartial, EmptyGroup.Replace("ArcadeEvents", "LobbyEvents", StringComparison.Ordinal), ListField.Replace("ArcadeEvents", "ScoreEvents", StringComparison.Ordinal)];

        return AnalyzerRunner<CustomEventsAnalyzer>.VerifyAsync(sources);
    }
}
