// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Events;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Events;

internal sealed class EventCaseAnalyzerTests
{
    private const string Usings = """
        using System.Collections.Generic;
        using KappaDuck.Khaos.Interop.SDL.Primitives;

        namespace KappaDuck.Khaos.Events;


        """;

    private const string GenericCase = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public readonly record struct KeyReleased<T>;
        """;

    private const string InternalCase = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        internal readonly record struct KeyReleased;
        """;

    private const string ListField = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public readonly record struct KeyReleased(List<int> Codes)
        {
            internal KeyReleased(in SDL_KeyboardEvent native)
                : this([native.Scancode])
            {
            }
        }
        """;

    private const string MutableCase = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public record struct KeyReleased;
        """;

    private const string NestedCase = Usings + """
        public static class Keys
        {
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased;
        }
        """;

    private const string PayloadWithoutConstructor = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public readonly record struct KeyReleased(int Code);
        """;

    private const string PublicConstructor = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public readonly record struct KeyReleased(int Code)
        {
            public KeyReleased(in SDL_KeyboardEvent native)
                : this(native.Scancode)
            {
            }
        }
        """;

    [Test]
    public async Task CaseInsideARangeShouldReportKHI016()
    {
        const string inside = Usings + """
            [KhaosEvent((SDL_EventType)0x8001)]
            public readonly record struct Inside;
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, EventSources.Cases, inside);

        await result.Reported.Should().BeEqualTo("Source2.cs(7,31): error KHI016: 'Inside' claims a native event type that 'CustomEvent' already converts");
    }

    [Test]
    public async Task GenericCaseShouldReportKHI013()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, GenericCase);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI013");
    }

    [Test]
    [Arguments(GenericCase)]
    [Arguments(InternalCase)]
    [Arguments(ListField)]
    [Arguments(MutableCase)]
    [Arguments(NestedCase)]
    [Arguments(PayloadWithoutConstructor)]
    [Arguments(PublicConstructor)]
    public async Task InvalidCaseShouldBeReportedAndLeftOutOfEvent(string invalid)
    {
        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, EventSources.Cases, invalid);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(EventSources.Framework, EventSources.Event, EventSources.Cases, invalid);

        await reported.Ids.Should().NotBeEmpty();
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("KeyReleased");
    }

    [Test]
    public async Task InternalCaseShouldReportKHI013()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, InternalCase);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI013");
    }

    [Test]
    public async Task ListFieldShouldReportKHI014()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, ListField);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI014: 'KeyReleased' holds 'Codes' of type System.Collections.Generic.List<int>; an event case holds only unmanaged values and strings");
    }

    [Test]
    public async Task MutableRecordStructShouldReportKHI013()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, MutableCase);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI013");
    }

    [Test]
    public async Task NativeConstructorTakingATypeOutsideSDLEventShouldReportKHI015()
    {
        const string source = Usings + """
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased(int Code)
            {
                internal KeyReleased(in int code)
                    : this(code + 1)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI015: 'KeyReleased' takes Int32, which is not the type of exactly one SDL_Event field");
    }

    [Test]
    public async Task NestedCaseShouldReportKHI013()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, NestedCase);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI013");
    }

    [Test]
    public async Task PayloadWithoutNativeConstructorShouldReportKHI015()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, PayloadWithoutConstructor);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI015: 'KeyReleased' has a payload but no internal constructor that takes its native event");
    }

    [Test]
    public async Task PublicNativeConstructorShouldReportKHI015()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, PublicConstructor);

        await result.Ids.Should().BeEqualTo("KHI015");
    }

    [Test]
    public async Task RangeEndingBeforeItStartsShouldReportKHI013()
    {
        const string source = Usings + """
            [KhaosEvent(SDL_EventType.Last, SDL_EventType.User)]
            public readonly record struct Backwards;
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI013: 'Backwards' covers a range of native types whose last type comes before its first");
    }

    [Test]
    public async Task SecondParameterOtherThanTheContextShouldReportKHI015()
    {
        const string source = Usings + """
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased(int Code)
            {
                internal KeyReleased(in SDL_KeyboardEvent native, in float scale)
                    : this((int)(native.Scancode * scale))
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI015");
    }

    [Test]
    public async Task StringFieldShouldReportNothing()
    {
        const string source = Usings + """
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct TextTyped(string Text)
            {
                internal TextTyped(in SDL_KeyboardEvent native)
                    : this(native.Scancode.ToString(System.Globalization.CultureInfo.InvariantCulture))
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task TwoCasesForOneTypeShouldReportKHI016OnTheSecond()
    {
        const string second = Usings + """
            [KhaosEvent(SDL_EventType.Quit)]
            public readonly record struct SecondQuit;
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, EventSources.Cases, second);

        await result.Reported.Should().BeEqualTo("Source2.cs(7,31): error KHI016: 'SecondQuit' claims a native event type that 'QuitRequested' already converts");
    }

    [Test]
    public async Task TwoConstructorsShouldReportKHI015()
    {
        const string source = Usings + """
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased(int Code)
            {
                internal KeyReleased(in SDL_KeyboardEvent native)
                    : this(native.Scancode)
                {
                }

                internal KeyReleased(long code)
                    : this((int)code)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI015");
    }

    [Test]
    public async Task ValidCasesShouldReportNothing()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, EventSources.Cases);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public Task VerifyAsyncShouldSnapshotTheDiagnostics()
    {
        string[] sources = [EventSources.Framework, MutableCase, ListField.Replace("KeyReleased", "Typed", StringComparison.Ordinal), PayloadWithoutConstructor.Replace("KeyReleased", "Pressed", StringComparison.Ordinal)];

        return AnalyzerRunner<EventCaseAnalyzer>.VerifyAsync(sources);
    }
}
