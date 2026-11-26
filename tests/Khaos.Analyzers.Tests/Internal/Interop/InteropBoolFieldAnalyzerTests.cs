// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

internal sealed class InteropBoolFieldAnalyzerTests
{
    [Test]
    public async Task BoolFieldInSDLStructShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal bool down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI007: Field 'SDL_KeyboardEvent.down' is declared as bool; in SDL a native boolean is stored as byte");
    }

    [Test]
    public async Task BoolFieldWithMarshalAsInWin32StructShouldReportKHI007()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct PAINTSTRUCT
            {
                [MarshalAs(UnmanagedType.Bool)]
                internal bool fErase;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(8,14): error KHI007: Field 'PAINTSTRUCT.fErase' is declared as bool; in Win32 a native boolean is stored as int");
    }

    [Test]
    public async Task BoolAutoPropertyInSDLStructShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal bool Down { get; set; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI007: Property 'SDL_KeyboardEvent.Down' is declared as bool; in SDL a native boolean is stored as byte");
    }

    [Test]
    public async Task CapturedPrimaryConstructorParameterShouldReportKHI007ByItsName()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent(bool down)
            {
                internal readonly byte Down() => down ? (byte)1 : (byte)0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(3,35): error KHI007: Field 'SDL_KeyboardEvent.down' is declared as bool; in SDL a native boolean is stored as byte");
    }

    [Test]
    [Arguments("struct")]
    [Arguments("record struct")]
    [Arguments("ref struct")]
    [Arguments("partial struct")]
    public async Task BoolFieldInEveryKindOfStructShouldReportKHI007(string kind)
    {
        Markup source = Markup.Parse($$"""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal {{kind}} SDL_KeyboardEvent
            {
                internal {|KHI007:bool|} down;
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task BoolAutoPropertyInWin32StructShouldReportKHI007()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct PAINTSTRUCT
            {
                internal {|KHI007:bool|} Erase { get; init; }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task BoolFieldKeywordPropertyShouldReportKHI007()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct PAINTSTRUCT
            {
                internal {|KHI007:bool|} Erase
                {
                    get => field;
                    set => field = value;
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PositionalBoolPropertyOfARecordStructShouldReportKHI007AtItsType()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal record struct SDL_KeyboardEvent({|KHI007:bool|} Down, byte Repeat);
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task FixedBoolBufferShouldReportKHI007()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_GamepadState
            {
                internal fixed {|KHI007:bool|} buttons[4];
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task EveryBoolFieldOfADeclarationShouldReportKHI007()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Flags
            {
                internal {|KHI007:{|KHI007:bool|}|} first, second;
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
        await result.Reported.Should().Contain("Field 'SDL_Flags.first'");
        await result.Reported.Should().Contain("Field 'SDL_Flags.second'");
    }

    [Test]
    public async Task BoolFieldInANestedStructShouldReportKHI007()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.Win32;

            internal static class User32
            {
                internal struct PAINTSTRUCT
                {
                    internal {|KHI007:bool|} fErase;
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task BoolFieldInPartialStructShouldReportKHI007Once()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal partial struct SDL_KeyboardEvent
            {
                internal {|KHI007:bool|} down;
            }
            """, """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal partial struct SDL_KeyboardEvent
            {
                internal byte repeat;
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task BoolFieldInGeneratedStructShouldReportKHI007()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL.Primitives
            {
                internal struct SDL_KeyboardEvent
                {
                    internal {|KHI007:bool|} down;
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task ByteAndIntFieldsShouldReportNothing()
    {
        const string sdl = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal byte down;
                internal fixed byte padding[3];
            }
            """;

        const string win32 = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct PAINTSTRUCT
            {
                internal int fErase;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(sdl, win32);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    [Arguments("bool*")]
    [Arguments("bool?")]
    public async Task PointerAndNullableBoolFieldsShouldReportNothing(string type)
    {
        string source = $$"""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal {{type}} down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task StaticAndConstBoolFieldsShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal const bool IsKeyboard = true;
                internal static bool Seen;
                internal static bool Tracked { get; set; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task BoolPropertyWithABodyShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal byte down;

                internal readonly bool IsDown => down != 0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PrimaryConstructorParameterNotCapturedShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent(bool down)
            {
                internal byte isDown = down ? (byte)1 : (byte)0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task BoolFieldInAClassShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal sealed class Subscription
            {
                internal bool active;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task BoolFieldOutsideInteropShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            internal struct KeyState
            {
                internal bool down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task BoolFieldInAnUnlistedInteropAreaShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Browser;

            internal struct KeyState
            {
                internal bool down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }
}
