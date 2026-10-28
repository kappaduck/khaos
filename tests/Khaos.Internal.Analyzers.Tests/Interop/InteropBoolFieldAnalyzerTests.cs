// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

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
        await result.Ids.Should().BeEqualTo("KHI007");
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
        await result.Ids.Should().BeEqualTo("KHI007");
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
        await result.Ids.Should().BeEqualTo("KHI007");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI007: Property 'SDL_KeyboardEvent.Down' is declared as bool; in SDL a native boolean is stored as byte");
    }

    [Test]
    public async Task BoolFieldKeywordPropertyShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct PAINTSTRUCT
            {
                internal bool Erase
                {
                    get => field;
                    set => field = value;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007");
        await result.Reported.Should().Contain("Property 'PAINTSTRUCT.Erase' is declared as bool; in Win32 a native boolean is stored as int");
    }

    [Test]
    public async Task FixedBoolBufferShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_GamepadState
            {
                internal fixed bool buttons[4];
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,20): error KHI007: Field 'SDL_GamepadState.buttons' is declared as bool; in SDL a native boolean is stored as byte");
    }

    [Test]
    public async Task EveryBoolFieldOfADeclarationShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Flags
            {
                internal bool first, second;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007, KHI007");
        await result.Reported.Should().Contain("Field 'SDL_Flags.first'");
        await result.Reported.Should().Contain("Field 'SDL_Flags.second'");
    }

    [Test]
    public async Task BoolFieldInARecordStructShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal record struct SDL_KeyboardEvent
            {
                internal bool down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007");
    }

    [Test]
    public async Task BoolFieldInANestedStructShouldReportKHI007()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32;

            internal static class User32
            {
                internal struct PAINTSTRUCT
                {
                    internal bool fErase;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007");
        await result.Reported.Should().Contain("Field 'PAINTSTRUCT.fErase'");
    }

    [Test]
    public async Task BoolFieldInPartialStructShouldReportKHI007Once()
    {
        const string first = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal partial struct SDL_KeyboardEvent
            {
                internal bool down;
            }
            """;

        const string second = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal partial struct SDL_KeyboardEvent
            {
                internal byte repeat;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(first, second);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007");
    }

    [Test]
    public async Task BoolFieldInGeneratedStructShouldReportKHI007()
    {
        const string source = """
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL.Primitives
            {
                internal struct SDL_KeyboardEvent
                {
                    internal bool down;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolFieldAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI007");
    }

    [Test]
    public async Task ByteAndIntFieldsShouldReportNothing()
    {
        const string sdl = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_KeyboardEvent
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
    }
}
