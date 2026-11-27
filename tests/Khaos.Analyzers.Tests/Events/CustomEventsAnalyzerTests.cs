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
        await result.Reported.Should().BeEqualTo("Source1.cs(7,32): error KHAOS002: 'ArcadeEvents' is marked [CustomEvents] but lists no events; list them as [CustomEvents(typeof(MyEvent))]");
    }

    [Test]
    [Arguments("typeof(CoinInserted), {|KHAOS005:typeof(CoinInserted)|}")]
    [Arguments("typeof(CoinInserted), typeof(GameOver), {|KHAOS005:typeof(CoinInserted)|}")]
    [Arguments("typeof(CoinInserted), {|KHAOS005:typeof(CoinInserted)|}, {|KHAOS005:typeof(CoinInserted)|}")]
    [Arguments("typeof(CoinInserted), {|KHAOS005:typeof(Coin)|}")]
    [Arguments("typeof(Menu.Started), {|KHAOS005:typeof(Game.Started)|}")]
    [Arguments("typeof(Game.Started), {|KHAOS005:typeof(Menu.Started)|}")]
    public async Task EventListedTwiceOrSharingANameShouldReportKHAOS005AndNotBeGenerated(string listed)
    {
        Markup source = Markup.Parse(CustomEventSources.Framework, "using Coin = Arcade.CoinInserted;\n" + Usings + $$"""
            public readonly record struct CoinInserted(int Credits);

            public readonly record struct GameOver(int Score);

            public static class Menu
            {
                public readonly record struct Started(int Credits);
            }

            public static class Game
            {
                public readonly record struct Started(int Level);
            }

            [CustomEvents({{listed}})]
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    public async Task EventsWhoseNamesDifferByCaseShouldReportNothing()
    {
        Markup source = Group("""
            public readonly record struct Coin(int Credits);

            public readonly record struct coin(int Cents);

            [CustomEvents(typeof(Coin), typeof(coin))]
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Ids.Should().BeEmpty();
        await generated.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task EventsWithTheSameNameShouldReportKHAOS005()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, SameNameEvents);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(15,41): error KHAOS005: 'Arcade.Game.Started' in 'ArcadeEvents' has the same name as 'Arcade.Menu.Started'; rename one of them");
    }

    [Test]
    [Arguments("public readonly struct {|KHAOS001:ArcadeEvents|};")]
    [Arguments("public partial struct {|KHAOS001:ArcadeEvents|};")]
    [Arguments("public readonly partial record struct {|KHAOS001:ArcadeEvents|};")]
    [Arguments("public readonly partial struct {|KHAOS001:ArcadeEvents|}<T>;")]
    public async Task GroupOfAnotherShapeShouldReportKHAOS001AndNotBeGenerated(string declaration)
    {
        Markup source = Group($$"""
            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            {{declaration}}
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("public static class Cabinet")]
    [Arguments("public readonly partial struct Cabinet")]
    public async Task NestedGroupShouldReportKHAOS001AndNotBeGenerated(string container)
    {
        Markup source = Group($$"""
            public readonly record struct CoinInserted(int Credits);

            {{container}}
            {
                [CustomEvents(typeof(CoinInserted))]
                public readonly partial struct {|KHAOS001:ArcadeEvents|};
            }
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("[CustomEvents(typeof(CoinInserted))] public readonly partial struct ArcadeEvents;")]
    [Arguments("[CustomEvents(typeof(CoinInserted))] internal readonly partial struct ArcadeEvents;")]
    [Arguments("[CustomEvents(typeof(CoinInserted))] public readonly partial struct ArcadeEvents; public readonly partial struct ArcadeEvents;")]
    public async Task ReadOnlyPartialStructShouldReportNothing(string declarations)
    {
        Markup source = Group($$"""
            public readonly record struct CoinInserted(int Credits);

            {{declarations}}
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Ids.Should().BeEmpty();
        await generated.GeneratedFiles.Count.Should().BeEqualTo(1);
    }

    [Test]
    public async Task FileLocalGroupShouldReportKHAOS001()
    {
        Markup source = Group("""
            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            file readonly partial struct {|KHAOS001:ArcadeEvents|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("file readonly record struct CoinInserted(int Credits);", "typeof(CoinInserted)", "internal", "must be public or internal")]
    [Arguments("file static class Cabinet { public readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)", "internal", "must be public or internal")]
    [Arguments("file readonly record struct CoinInserted(int Credits);", "typeof(CoinInserted)", "public", "is less accessible than the group")]
    public async Task FileLocalEventShouldReportKHAOS003AndNotBeGenerated(string declaration, string listed, string group, string reason)
    {
        Markup source = Group($$"""
            {{declaration}}

            [CustomEvents({|KHAOS003:{{listed}}|})]
            {{group}} readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await reported.Reported.Should().Contain(reason);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    public async Task PartialGroupShouldBeReportedWhereItsAttributeIs()
    {
        Markup source = Group("""
            public readonly partial struct ArcadeEvents;

            [CustomEvents]
            public readonly partial struct {|KHAOS002:ArcadeEvents|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("[CustomEvents(42)]")]
    [Arguments("[CustomEvents(\"CoinInserted\")]")]
    public async Task AttributeThatDoesNotBindShouldBeLeftToTheCompiler(string attribute)
    {
        Markup source = Group($$"""
            {{attribute}}
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().Contain(": error CS");
        await reported.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task ShapeShouldStopTheOtherRules()
    {
        Markup source = Group("""
            public sealed class CoinInserted;

            [CustomEvents(typeof(CoinInserted), typeof(CoinInserted))]
            public readonly struct {|KHAOS001:TryGet|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("public sealed class CoinInserted;", "typeof(CoinInserted)")]
    [Arguments("public record struct CoinInserted(int Credits);", "typeof(CoinInserted)")]
    [Arguments("public readonly struct CoinInserted;", "typeof(CoinInserted)")]
    [Arguments("public sealed record CoinInserted(int Credits);", "typeof(CoinInserted)")]
    [Arguments("public readonly record struct CoinInserted<T>(T Credits);", "typeof(CoinInserted<int>)")]
    [Arguments("public enum CoinInserted { One }", "typeof(CoinInserted)")]
    [Arguments("public interface CoinInserted;", "typeof(CoinInserted)")]
    [Arguments("", "typeof(int)")]
    [Arguments("", "typeof(int[])")]
    public async Task EventOfAnotherShapeShouldReportKHAOS003AndNotBeGenerated(string declaration, string listed)
    {
        Markup source = Group($$"""
            {{declaration}}

            [CustomEvents({|KHAOS003:{{listed}}|})]
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await reported.Reported.Should().Contain("is not a non-generic readonly record struct");
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("internal readonly record struct CoinInserted(int Credits);", "typeof(CoinInserted)")]
    [Arguments("public static class Cabinet { internal readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)")]
    [Arguments("internal static class Cabinet { public readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)")]
    [Arguments("public class Cabinet { protected internal readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)")]
    public async Task EventLessVisibleThanAPublicGroupShouldReportKHAOS003(string declaration, string listed)
    {
        Markup source = Group($$"""
            {{declaration}}

            [CustomEvents({|KHAOS003:{{listed}}|})]
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await reported.Reported.Should().Contain("is less accessible than the group");
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("public readonly record struct HighScores(List<int> Scores);", "Scores")]
    [Arguments("public readonly record struct HighScores(int[] Scores);", "Scores")]
    [Arguments("public readonly record struct HighScores { public readonly object Board; }", "Board")]
    public async Task EventHoldingAReferenceShouldReportKHAOS003(string declaration, string member)
    {
        Markup source = Group($$"""
            {{declaration}}

            [CustomEvents({|KHAOS003:typeof(HighScores)|})]
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await reported.Reported.Should().Contain($"holds '{member}' of type");
    }

    [Test]
    [Arguments("internal readonly record struct CoinInserted(int Credits);", "typeof(CoinInserted)", "internal")]
    [Arguments("public static class Cabinet { internal readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)", "internal")]
    [Arguments("public class Cabinet { protected internal readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)", "internal")]
    [Arguments("public static class Cabinet { public readonly record struct CoinInserted(int Credits); }", "typeof(Cabinet.CoinInserted)", "public")]
    [Arguments("public readonly record struct CoinInserted(string Player, Credit Credit) { public static readonly object Shared = new(); } public readonly record struct Credit(int Amount);", "typeof(CoinInserted)", "public")]
    public async Task ValidEventShouldReportNothing(string declaration, string listed, string group)
    {
        Markup source = Group($$"""
            {{declaration}}

            [CustomEvents({{listed}})]
            {{group}} readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Ids.Should().BeEmpty();
        await generated.CompilationErrors.Should().BeEmpty();
        await generated.GeneratedFiles.Count.Should().BeEqualTo(1);
    }

    [Test]
    public async Task EventsListedAsOneArrayShouldBeReportedOnTheGroup()
    {
        Markup source = Group("""
            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(new[] { typeof(CoinInserted), typeof(int) })]
            public readonly partial struct {|KHAOS003:ArcadeEvents|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("[CustomEvents]")]
    [Arguments("[CustomEvents()]")]
    [Arguments("[CustomEvents(new System.Type[0])]")]
    [Arguments("[CustomEvents(null)]")]
    public async Task GroupListingNoEventShouldReportKHAOS002AndNotBeGenerated(string attribute)
    {
        Markup source = Group($$"""
            {{attribute}}
            public readonly partial struct {|KHAOS002:ArcadeEvents|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments("public sealed class CoinInserted;", "[CustomEvents({|KHAOS003:typeof(CoinInserted)|})]", 0)]
    [Arguments("", "[CustomEvents(typeof(Missing))]", 0)]
    [Arguments("public readonly record struct CoinInserted(int Credits);", "[CustomEvents(typeof(Missing), typeof(CoinInserted))]", 1)]
    public async Task GroupWhoseEventsAreAllRefusedShouldNotReportKHAOS002(string declaration, string attribute, int files)
    {
        Markup source = Group($$"""
            {{declaration}}

            {{attribute}}
            public readonly partial struct ArcadeEvents;
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(files);
    }

    [Test]
    [Arguments("Is")]
    [Arguments("TryGet")]
    [Arguments("Equals")]
    [Arguments("GetHashCode")]
    [Arguments("ToString")]
    [Arguments("GetType")]
    [Arguments("ReferenceEquals")]
    public async Task GroupNamedLikeAMemberOfCustomEventShouldWarnKHAOS004AndStillBeGenerated(string name)
    {
        Markup source = Group($$"""
            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            public readonly partial struct {|KHAOS004:{{name}}|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(1);
    }

    [Test]
    [Arguments("IsReady")]
    [Arguments("MemberwiseClone")]
    [Arguments("PrintMembers")]
    [Arguments("Value")]
    public async Task GroupNamedLikeNoPublicMemberOfCustomEventShouldReportNothing(string name)
    {
        Markup source = Group($$"""
            public readonly record struct CoinInserted(int Credits);

            [CustomEvents(typeof(CoinInserted))]
            public readonly partial struct {{name}};
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Ids.Should().BeEmpty();
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
    public async Task RefusedEventShouldNotTakeItsName()
    {
        Markup source = Markup.Parse(CustomEventSources.Framework, """
            using KappaDuck.Khaos.Events;

            namespace Arcade.Menu
            {
                public sealed class Started;
            }

            namespace Arcade.Game
            {
                public readonly record struct Started(int Level);
            }

            namespace Arcade
            {
                [CustomEvents({|KHAOS003:typeof(Menu.Started)|}, typeof(Game.Started))]
                public readonly partial struct ArcadeEvents;
            }
            """);

        AnalyzerResult reported = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
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

    [Test]
    [Arguments(255, false)]
    [Arguments(256, true)]
    [Arguments(300, true)]
    public async Task GroupOfMoreThan255EventsShouldReportKHAOS006AndNotBeGenerated(int count, bool reported)
    {
        string group = reported ? "{|KHAOS006:ArcadeEvents|}" : "ArcadeEvents";
        Markup source = Group(ManyEvents(count, group));

        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<CustomEventsGenerator>.Run(source.Sources.AsSpan());

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
        await generated.GeneratedFiles.Count.Should().BeEqualTo(reported ? 0 : 1);
    }

    [Test]
    public async Task GroupOfMoreThan255EventsShouldReportKHAOS006WithItsMessage()
    {
        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, Usings + ManyEvents(256, "ArcadeEvents"));

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(264,32): error KHAOS006: 'ArcadeEvents' holds 256 events; a group holds at most 255, so split it in two");
    }

    [Test]
    public async Task RefusedEventShouldNotCountInTheMessageOfKHAOS006()
    {
        string source = Usings + "public sealed class CoinBag;\n\n" + ManyEvents(256, "ArcadeEvents").Replace("[CustomEvents(", "[CustomEvents(typeof(CoinBag), ", StringComparison.Ordinal);

        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(CustomEventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().Contain("'ArcadeEvents' holds 256 events");
    }

    [Test]
    public async Task RefusedEventShouldNotCountTowardKHAOS006()
    {
        Markup source = Group("public sealed class CoinBag;\n\n" + ManyEvents(255, "ArcadeEvents").Replace("[CustomEvents(", "[CustomEvents({|KHAOS003:typeof(CoinBag)|}, ", StringComparison.Ordinal));

        AnalyzerResult result = await AnalyzerRunner<CustomEventsAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    private static Markup Group(string declarations)
        => Markup.Parse(CustomEventSources.Framework, Usings + declarations);

    private static string ManyEvents(int count, string group)
    {
        string events = string.Join("\n", Enumerable.Range(0, count).Select(static i => $"public readonly record struct Coin{i}(int Credits);"));
        string listed = string.Join(", ", Enumerable.Range(0, count).Select(static i => $"typeof(Coin{i})"));

        return $"{events}\n\n[CustomEvents({listed})]\npublic readonly partial struct {group};\n";
    }
}
