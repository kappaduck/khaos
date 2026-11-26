// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Events;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Events;

internal sealed class EventCaseAnalyzerTests
{
    private const string Usings = """
        using System.Collections.Generic;
        using KappaDuck.Khaos.Interop.SDL.Primitives;

        namespace KappaDuck.Khaos.Events;


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

    private const string Members = """
        public readonly record struct Label(string Text);

        public readonly record struct Size(int Width, int Height);

        public enum Side
        {
            Left,
            Right
        }

        """;

    private const string MutableCase = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public record struct KeyReleased;
        """;

    private const string PayloadWithoutConstructor = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public readonly record struct KeyReleased(int Code);
        """;

    private const string PrivateConstructor = Usings + """
        [KhaosEvent(SDL_EventType.KeyUp)]
        public readonly record struct KeyReleased(int Code)
        {
            private KeyReleased(in SDL_KeyboardEvent native)
                : this(native.Scancode)
            {
            }
        }
        """;

    [Test]
    public async Task BackwardRangeOfAnotherShapeShouldReportTheShapeOnly()
    {
        Markup source = Markup.Parse(EventSources.Framework, Usings + """
            [KhaosEvent(SDL_EventType.Last, SDL_EventType.User)]
            public record struct {|KHI013:Backwards|};
            """);

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
        await result.Reported.Should().Contain("declare it as a public, non-generic, top-level readonly record struct");
    }

    [Test]
    [Arguments("[KhaosEvent(SDL_EventType.Last, SDL_EventType.User)]")]
    [Arguments("[KhaosEvent(SDL_EventType.KeyUp, SDL_EventType.KeyDown)]")]
    public async Task BackwardRangeShouldReportKHI013AndBeLeftOutOfEvent(string attribute)
    {
        Markup source = Case($$"""
            {{attribute}}
            public readonly record struct {|KHI013:Backwards|};
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("Backwards");
    }

    [Test]
    public async Task BackwardRangeShouldReportKHI013WithItsMessage()
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
    [Arguments("[KhaosEvent(SDL_EventType.Quit)] public readonly record struct QuitRequested; [KhaosEvent(SDL_EventType.Quit)] public readonly record struct {|KHI016:SecondQuit|};", "QuitRequested", "SecondQuit")]
    [Arguments("[KhaosEvent(SDL_EventType.Quit)] public readonly record struct {|KHI016:QuitRequested|}; [KhaosEvent(SDL_EventType.Quit)] public readonly record struct AQuit;", "AQuit", "QuitRequested")]
    [Arguments("[KhaosEvent(SDL_EventType.User, SDL_EventType.Last)] public readonly record struct CustomEvent; [KhaosEvent((SDL_EventType)0x8001)] public readonly record struct {|KHI016:Inside|};", "CustomEvent", "Inside")]
    [Arguments("[KhaosEvent(SDL_EventType.KeyDown, SDL_EventType.KeyUp)] public readonly record struct Keys; [KhaosEvent(SDL_EventType.KeyUp)] public readonly record struct {|KHI016:AKeyReleased|};", "Keys", "AKeyReleased")]
    [Arguments("[KhaosEvent(SDL_EventType.KeyDown)] public readonly record struct KeyPressed; [KhaosEvent(SDL_EventType.KeyDown, SDL_EventType.KeyUp)] public readonly record struct {|KHI016:ZKeys|};", "KeyPressed", "ZKeys")]
    [Arguments("[KhaosEvent((SDL_EventType)0x9000, (SDL_EventType)0x9002)] public readonly record struct Low; [KhaosEvent((SDL_EventType)0x9002, (SDL_EventType)0x9004)] public readonly record struct {|KHI016:High|};", "Low", "High")]
    [Arguments("[KhaosEvent(SDL_EventType.Quit)] public readonly record struct AQuit; [KhaosEvent(SDL_EventType.Quit, SDL_EventType.WindowResized)] public readonly record struct {|KHI016:BRange|}; [KhaosEvent(SDL_EventType.WindowResized)] public readonly record struct WindowResized;", "WindowResized", "BRange")]
    public async Task ClaimedTypeShouldReportKHI016OnTheCaseLeftOutOfEvent(string declarations, string kept, string dropped)
    {
        Markup source = Markup.Parse(EventSources.Framework, EventSources.Event, Usings + declarations);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        string union = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan()).Source(EventUnionGenerator.EventHintName);

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await union.Should().Contain($"public Event(global::KappaDuck.Khaos.Events.{kept} value)");
        await union.Should().NotContain($"public Event(global::KappaDuck.Khaos.Events.{dropped} value)");
    }

    [Test]
    public async Task AdjacentRangesShouldReportNothing()
    {
        const string source = Usings + """
            [KhaosEvent((SDL_EventType)0x9000, (SDL_EventType)0x9001)]
            public readonly record struct Low;

            [KhaosEvent((SDL_EventType)0x9002, (SDL_EventType)0x9003)]
            public readonly record struct High;
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task InvalidCaseOnAClaimedTypeShouldReportOnlyItsOwnRule()
    {
        Markup source = Markup.Parse(EventSources.Framework, Usings + """
            [KhaosEvent(SDL_EventType.Quit)]
            public readonly record struct QuitRequested;

            [KhaosEvent(SDL_EventType.Quit)]
            public readonly record struct {|KHI015:AQuit|}(int Code);
            """);

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("public record struct {|KHI013:KeyReleased|};")]
    [Arguments("public readonly struct {|KHI013:KeyReleased|};")]
    [Arguments("public struct {|KHI013:KeyReleased|};")]
    [Arguments("public readonly ref struct {|KHI013:KeyReleased|};")]
    [Arguments("internal readonly record struct {|KHI013:KeyReleased|};")]
    [Arguments("file readonly record struct {|KHI013:KeyReleased|};")]
    [Arguments("public readonly record struct {|KHI013:KeyReleased|}<T>;")]
    public async Task CaseOfAnotherShapeShouldReportKHI013AndBeLeftOutOfEvent(string declaration)
    {
        Markup source = Case($$"""
            [KhaosEvent(SDL_EventType.KeyUp)]
            {{declaration}}
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("KeyReleased");
    }

    [Test]
    [Arguments("")]
    [Arguments("internal KeyReleased(in SDL_KeyboardEvent native) : this(native.Scancode) { } internal KeyReleased(long code) : this((int)code) { }")]
    [Arguments("private KeyReleased(in SDL_KeyboardEvent native) : this(native.Scancode) { }")]
    [Arguments("public KeyReleased() : this(0) { }")]
    [Arguments("internal KeyReleased(SDL_KeyboardEvent native) : this(native.Scancode) { }")]
    [Arguments("internal KeyReleased(ref readonly SDL_KeyboardEvent native) : this(native.Scancode) { }")]
    [Arguments("internal KeyReleased(ref SDL_KeyboardEvent native) : this(native.Scancode) { }")]
    [Arguments("internal KeyReleased(in int code) : this(code) { }")]
    [Arguments("internal KeyReleased(in SDL_KeyboardEvent native, in float scale) : this((int)(native.Scancode * scale)) { }")]
    [Arguments("internal KeyReleased(in SDL_KeyboardEvent native, EventContext context) : this(native.Scancode) { }")]
    [Arguments("internal KeyReleased(SDL_KeyboardEvent native, in EventContext context) : this(native.Scancode) { }")]
    [Arguments("internal KeyReleased(in SDL_KeyboardEvent native, in EventContext context, in int extra) : this(extra) { }")]
    public async Task ConstructorFaultShouldReportKHI015AndBeLeftOutOfEvent(string constructors)
    {
        Markup source = Case($$"""
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct {|KHI015:KeyReleased|}(int Code)
            {
                {{constructors}}
            }
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("KeyReleased");
    }

    [Test]
    public async Task ConstructorOfAnotherContractShouldReportKHI015WithItsMessage()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, PrivateConstructor);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI015: 'KeyReleased' must declare its native constructor internal, taking 'in' the type of one SDL_Event field, then optionally 'in EventContext'");
    }

    [Test]
    [Arguments("public readonly record struct KeyReleased;")]
    [Arguments("public readonly record struct KeyReleased { public static readonly int Shared = 1; public const int Max = 3; }")]
    [Arguments("public readonly record struct KeyReleased { internal KeyReleased(in SDL_KeyboardEvent native) => Code = native.Scancode; internal int Code { get; } }")]
    [Arguments("public readonly record struct KeyReleased(int Code) { internal KeyReleased(in SDL_KeyboardEvent native) : this(native.Scancode) { } }")]
    [Arguments("public readonly record struct KeyReleased(int Code) { internal KeyReleased(in SDL_KeyboardEvent native, in EventContext context) : this((int)(native.Scancode * context.ContentScale)) { } }")]
    public async Task NativeConstructorShouldReportNothing(string declaration)
    {
        Markup source = Case($$"""
            [KhaosEvent(SDL_EventType.KeyUp)]
            {{declaration}}
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Ids.Should().BeEmpty();
        await generated.Source(EventUnionGenerator.EventHintName).Should().Contain("KeyReleased");
    }

    [Test]
    [Arguments("[FieldOffset(0)] public safe SDL_KeyboardEvent Repeat;", "{|KHI015:KeyReleased|}")]
    [Arguments("public static SDL_KeyboardEvent Default;", "KeyReleased")]
    public async Task OtherFieldOfSDLEventShouldCountOnlyWhenItIsAnInstanceField(string field, string name)
    {
        string framework = EventSources.Framework.Replace("public safe SDL_UserEvent User;", $"public safe SDL_UserEvent User; {field}", StringComparison.Ordinal);

        Markup source = Markup.Parse(framework, Usings + $$"""
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct {{name}}(int Code)
            {
                internal KeyReleased(in SDL_KeyboardEvent native)
                    : this(native.Scancode)
                {
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task SDLEventMissingShouldReportKHI015()
    {
        string framework = EventSources.Framework.Replace("internal struct SDL_Event", "internal struct SDL_Events", StringComparison.Ordinal);

        Markup source = Markup.Parse(framework, Usings + """
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct {|KHI015:KeyReleased|}(int Code)
            {
                internal KeyReleased(in SDL_KeyboardEvent native)
                    : this(native.Scancode)
                {
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("public List<int> Codes { get; }")]
    [Arguments("public int[] Codes { get; }")]
    [Arguments("public object Codes { get; }")]
    [Arguments("public System.Action Codes { get; }")]
    [Arguments("public Label Codes { get; }")]
    [Arguments("public readonly List<int> Codes;")]
    [Arguments("public List<int> Codes { get; } = [];")]
    public async Task ReferenceMemberShouldReportKHI014AndBeLeftOutOfEvent(string member)
    {
        Markup source = Case($$"""
            {{Members}}
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct {|KHI014:KeyReleased|}
            {
                internal KeyReleased(in SDL_KeyboardEvent native)
                {
                }

                {{member}}
            }
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await reported.Reported.Should().Contain("holds 'Codes' of type");
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("KeyReleased");
    }

    [Test]
    [Arguments("public static readonly object Shared = new();")]
    [Arguments("public static object Shared { get; } = new();")]
    [Arguments("public const object Nothing = null;")]
    [Arguments("public const string Title = \"Title\";")]
    [Arguments("public Size Codes { get; }")]
    [Arguments("public int? Codes { get; }")]
    [Arguments("public Side Codes { get; }")]
    [Arguments("public bool Codes { get; }")]
    [Arguments("public string Codes { get; }")]
    public async Task ValueStringOrStaticMemberShouldReportNothing(string member)
    {
        Markup source = Case($$"""
            {{Members}}
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased
            {
                internal KeyReleased(in SDL_KeyboardEvent native)
                {
                }

                public string Text { get; }

                {{member}}
            }
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Ids.Should().BeEmpty();
        await generated.Source(EventUnionGenerator.EventHintName).Should().Contain("KeyReleased");
    }

    [Test]
    public async Task ListFieldShouldReportKHI014()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, ListField);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI014: 'KeyReleased' holds 'Codes' of type System.Collections.Generic.List<int>; an event case holds only unmanaged values and strings");
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
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI015: 'KeyReleased' takes int, which is not the type of exactly one SDL_Event field");
    }

    [Test]
    [Arguments("public partial record struct KeyReleased;", "[KhaosEvent(SDL_EventType.KeyUp)] public partial record struct {|KHI013:KeyReleased|};")]
    [Arguments("public readonly partial record struct ZKeyPressed;", "[KhaosEvent(SDL_EventType.KeyDown)] public readonly partial record struct {|KHI016:ZKeyPressed|};")]
    public async Task PartialCaseShouldBeReportedWhereItsAttributeIs(string first, string second)
    {
        Markup source = Case($"""
            {first}

            {second}
            """);

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("public static class Keys")]
    [Arguments("public readonly record struct Keys")]
    public async Task NestedCaseShouldReportKHI013AndBeLeftOutOfEvent(string container)
    {
        Markup source = Case($$"""
            {{container}}
            {
                [KhaosEvent(SDL_EventType.KeyUp)]
                public readonly record struct {|KHI013:KeyReleased|};
            }
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().BeEmpty();
        await reported.Located.Should().BeEqualTo(source.Expected);
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("KeyReleased");
    }

    [Test]
    public async Task PayloadWithoutNativeConstructorShouldReportKHI015()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, PayloadWithoutConstructor);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI015: 'KeyReleased' has a payload but no internal constructor that takes its native event");
    }

    [Test]
    public async Task ReferenceFieldWithoutConstructorShouldReportKHI014AndKHI015()
    {
        Markup source = Markup.Parse(EventSources.Framework, Usings + """
            [KhaosEvent(SDL_EventType.KeyUp)]
            public readonly record struct {|KHI014:{|KHI015:KeyReleased|}|}(List<int> Codes);
            """);

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task RangeOfOneTypeShouldReportNothing()
    {
        const string source = Usings + """
            [KhaosEvent(SDL_EventType.KeyUp, SDL_EventType.KeyUp)]
            public readonly record struct KeyReleased;
            """;

        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task ShapeShouldReportKHI013WithItsMessage()
    {
        AnalyzerResult result = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(EventSources.Framework, MutableCase);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source1.cs(7,22): error KHI013: 'KeyReleased' is marked [KhaosEvent]; declare it as a public, non-generic, top-level readonly record struct");
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
        await result.Reported.Should().BeEqualTo("Source1.cs(7,31): error KHI015: 'KeyReleased' declares more than one constructor besides its positional one");
    }

    [Test]
    [Arguments("[KhaosEvent(SDL_EventType.Missing)]")]
    [Arguments("[KhaosEvent(Native.Dynamic)]")]
    [Arguments("[KhaosEvent(Native.Dynamic, SDL_EventType.Last)]")]
    [Arguments("[KhaosEvent(SDL_EventType.User, Native.Dynamic)]")]
    public async Task UnreadableNativeTypeShouldBeLeftToTheCompiler(string attribute)
    {
        Markup source = Case($$"""
            internal static class Native
            {
                internal static readonly SDL_EventType Dynamic = SDL_EventType.KeyUp;
            }

            {{attribute}}
            public readonly record struct KeyReleased;
            """);

        AnalyzerResult reported = await AnalyzerRunner<EventCaseAnalyzer>.RunAsync(source);
        GeneratorResult generated = GeneratorRunner<EventUnionGenerator>.Run(source.Sources.AsSpan());

        await reported.CompilationErrors.Should().Contain(": error CS");
        await reported.Ids.Should().BeEmpty();
        await generated.Source(EventUnionGenerator.EventHintName).Should().NotContain("KeyReleased");
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

    private static Markup Case(string declaration)
        => Markup.Parse(EventSources.Framework, EventSources.Event, EventSources.Cases, Usings + declaration);
}
