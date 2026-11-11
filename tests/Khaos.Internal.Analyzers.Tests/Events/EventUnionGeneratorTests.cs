// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Events;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Events;

internal sealed class EventUnionGeneratorTests
{
    private const string Converter = EventUnionGenerator.ConverterHintName;
    private const string Union = EventUnionGenerator.EventHintName;

    [Test]
    public async Task EditOfACaseConstructorBodyShouldReuseEveryStep()
    {
        string[] sources = [EventSources.Framework, EventSources.Event, EventSources.Cases];
        string[] edited = [EventSources.Framework, EventSources.Event, EventSources.Cases.Replace("native.Scancode", "native.Scancode + 0", StringComparison.Ordinal)];
        string[] reused = [EventUnionGenerator.CasesTracking, EventUnionGenerator.UnionTracking];

        string failures = IncrementalChecks.Edit<EventUnionGenerator>(sources, edited, reused, []);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task EditOfAnUnrelatedFileShouldReuseEveryStep()
    {
        string[] sources = [EventSources.Framework, EventSources.Event, EventSources.Cases, "public static class Score;"];
        string[] edited = [EventSources.Framework, EventSources.Event, EventSources.Cases, "public static class Score { public const int Best = 3; }"];
        string[] reused = [EventUnionGenerator.CasesTracking, EventUnionGenerator.UnionTracking];

        string failures = IncrementalChecks.Edit<EventUnionGenerator>(sources, edited, reused, []);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task EditThatAddsACaseShouldRecomputeTheUnion()
    {
        const string released = """
            using KappaDuck.Khaos.Interop.SDL.Primitives;

            namespace KappaDuck.Khaos.Events;

            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased;
            """;

        string[] sources = [EventSources.Framework, EventSources.Event, EventSources.Cases, "namespace KappaDuck.Khaos.Events;"];
        string[] edited = [EventSources.Framework, EventSources.Event, EventSources.Cases, released];
        string[] recomputed = [EventUnionGenerator.UnionTracking];

        string failures = IncrementalChecks.Edit<EventUnionGenerator>(sources, edited, [], recomputed);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task RerunShouldReuseEveryStep()
    {
        string[] sources = [EventSources.Framework, EventSources.Event, EventSources.Cases];

        string failures = IncrementalChecks.Rerun<EventUnionGenerator>(sources, EventUnionGenerator.CasesTracking, EventUnionGenerator.UnionTracking);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldCompileTypeAndPropertyPatternsOnEvent()
    {
        const string usage = """
            using KappaDuck.Khaos.Events;

            public static class Usage
            {
                public static int Match(in Event e) => e switch
                {
                    QuitRequested => 1,
                    WindowResized { Width: > 100 } resized => resized.Height,
                    KeyPressed { Code: 41 } => 2,
                    CustomEvent => 3,
                    _ => 0
                };
            }
            """;

        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases, usage);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldFindCasesThroughAConditionalAttribute()
    {
        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases);

        await result.Source(Union).Should().Contain("public Event(global::KappaDuck.Khaos.Events.QuitRequested value)");
    }

    [Test]
    public async Task RunShouldGenerateCodeThatCompiles()
    {
        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases);

        await result.CompilationErrors.Should().BeEmpty();
    }

    [Test]
    public async Task RunShouldGenerateNothingWithoutCases()
    {
        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event);

        await result.GeneratedFiles.Count.Should().BeEqualTo(0);
    }

    [Test]
    public async Task RunShouldKeepTheFirstOfTwoCasesForOneType()
    {
        const string second = """
            using KappaDuck.Khaos.Interop.SDL.Primitives;

            namespace KappaDuck.Khaos.Events;

            [KhaosEvent(SDL_EventType.Quit)]
            public readonly record struct SecondQuit;
            """;

        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases, second);

        await result.Source(Converter).Should().NotContain("SecondQuit");
    }

    [Test]
    public async Task RunShouldMatchARangeWithARelationalPattern()
    {
        const string expected = "case >= global::KappaDuck.Khaos.Interop.SDL.Primitives.SDL_EventType.User and <= global::KappaDuck.Khaos.Interop.SDL.Primitives.SDL_EventType.Last:";

        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases);

        await result.Source(Converter).Should().Contain(expected);
    }

    [Test]
    public async Task RunShouldOrderCasesByNativeTypeWhateverTheSourceOrder()
    {
        const string first = """
            using KappaDuck.Khaos.Interop.SDL.Primitives;

            namespace KappaDuck.Khaos.Events;

            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct AKeyReleased;
            """;

        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, first, EventSources.Cases);
        string union = result.Source(Union);

        int keyPressed = union.IndexOf("public Event(global::KappaDuck.Khaos.Events.KeyPressed value)", StringComparison.Ordinal);
        int keyReleased = union.IndexOf("public Event(global::KappaDuck.Khaos.Events.AKeyReleased value)", StringComparison.Ordinal);

        await keyPressed.Should().BeLessThan(keyReleased);
    }

    [Test]
    public async Task RunShouldPassOnlyTheNativeEventToACaseWithoutContext()
    {
        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases);

        await result.Source(Converter).Should().Contain("new global::KappaDuck.Khaos.Events.KeyPressed(in native.Key)");
    }

    [Test]
    public async Task RunShouldPassTheContextToACaseThatTakesIt()
    {
        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases);

        await result.Source(Converter).Should().Contain("new global::KappaDuck.Khaos.Events.WindowResized(in native.Window, in context)");
    }

    [Test]
    public async Task RunShouldSkipAnInvalidCaseWithoutReportingIt()
    {
        const string invalid = """
            using KappaDuck.Khaos.Interop.SDL.Primitives;

            namespace KappaDuck.Khaos.Events;

            [KhaosEvent(SDL_EventType.KeyUp)]
            public sealed class KeyReleased;
            """;

        GeneratorResult result = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases, invalid);

        await result.Ids.Should().BeEmpty();
        await result.Source(Union).Should().NotContain("KeyReleased");
    }

    [Test]
    public Task VerifyAsyncShouldSnapshotTheUnionAndTheConverter()
    {
        string[] sources = [EventSources.Framework, EventSources.Event, EventSources.Cases];

        return GeneratorRunner<EventUnionGenerator>.VerifyAsync(sources);
    }
}
