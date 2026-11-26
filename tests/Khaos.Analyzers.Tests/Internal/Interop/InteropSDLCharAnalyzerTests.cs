// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

internal sealed class InteropSDLCharAnalyzerTests
{
    [Test]
    public async Task CharPointerFieldInSDLStructShouldReportKHI011()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_TextInputEvent
            {
                internal uint type;
                internal char* text;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(6,14): error KHI011: Field 'SDL_TextInputEvent.text' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CharParameterShouldReportKHI011()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_Window;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_SetWindowTitle")]
                internal static partial byte SetWindowTitle(SDL_Window* window, char* title);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(10,69): error KHI011: Parameter 'title' of 'SetWindowTitle' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CharReturnShouldReportKHI011()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_GetError")]
                internal static partial char* GetError();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(8,29): error KHI011: The return value of 'GetError' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CharAutoPropertyShouldReportKHI011Once()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Key
            {
                internal char Character { get; set; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI011: Property 'SDL_Key.Character' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CharEventShouldReportKHI011()
    {
        const string source = """
            using System;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal sealed class TextInput
            {
                internal event Action<char>? Typed;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI011: Event 'TextInput.Typed' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CapturedPrimaryConstructorParameterShouldReportKHI011OnceAsAParameter()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Key(char key)
            {
                internal readonly int Code() => key;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(3,25): error KHI011: Parameter 'key' of 'SDL_Key' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    [Arguments("internal {|KHI011:char|} key;")]
    [Arguments("internal static {|KHI011:char|} separator;")]
    [Arguments("internal {|KHI011:char**|} lines;")]
    [Arguments("internal fixed {|KHI011:char|} name[32];")]
    [Arguments("internal {|KHI011:char?|} pending;")]
    [Arguments("internal {|KHI011:char[]|} keys;")]
    [Arguments("internal {|KHI011:delegate* unmanaged<char*, void>|} log;")]
    [Arguments("internal {|KHI011:delegate* unmanaged<int, char>|} next;")]
    [Arguments("internal event {|KHI011:System.Action<char>|} Typed { add { } remove { } }")]
    [Arguments("internal readonly {|KHI011:char*|} Text => null;")]
    [Arguments("internal {|KHI011:char|} Key { get; init; }")]
    public async Task CharInAnSDLStructShouldReportKHI011(string member)
    {
        Markup source = Markup.Parse($$"""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Keyboard
            {
                {{member}}
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("internal static {|KHI011:char|} Next() => 'a';")]
    [Arguments("internal static void Read(out {|KHI011:char|} key) => key = 'a';")]
    [Arguments("internal static void Log({|KHI011:char[]|} message) { }")]
    [Arguments("internal static void Log({|KHI011:ReadOnlySpan<char>|} message) { }")]
    [Arguments("internal static void Log({|KHI011:delegate* unmanaged<char*, void>|} callback) { }")]
    [Arguments("internal static void Log({|KHI011:delegate* unmanaged<int, char*, void>|} callback) { }")]
    [Arguments("internal static void Map({|KHI011:Func<int, char>|} map) { }")]
    [Arguments("[LibraryImport(\"SDL3\", EntryPoint = \"SDL_Log\")] internal static partial void Log({|KHI011:char[]|} message);")]
    public async Task CharInAnSDLMethodShouldReportKHI011(string member)
    {
        Markup source = Markup.Parse(InteropSources.SDL($$"""
                {{member}}
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task CharInADelegateShouldReportKHI011()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL;

            internal delegate {|KHI011:char*|} SDL_Format({|KHI011:char*|} message);
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
        await result.Reported.Should().Contain("The return value of 'SDL_Format' uses char");
        await result.Reported.Should().Contain("Parameter 'message' of 'SDL_Format' uses char");
    }

    [Test]
    public async Task CharInALocalFunctionShouldReportKHI011()
    {
        Markup source = Markup.Parse(InteropSources.SDL("""
                internal static int Count(byte first)
                {
                    return Next((char)first) == 'a' ? 1 : 0;

                    static {|KHI011:char|} Next({|KHI011:char|} value) => value;
                }
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PositionalCharOfARecordStructShouldReportKHI011ForTheParameterAndTheProperty()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal record struct SDL_Key({|KHI011:{|KHI011:char|}|} Key);
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task CharInAFileMarkedGeneratedShouldReportKHI011()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL.Primitives
            {
                internal struct SDL_TextInputEvent
                {
                    internal {|KHI011:char*|} text;
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task CharParameterWithItsGeneratedHalfShouldReportKHI011Once()
    {
        const string generated = """
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL
            {
                internal static partial class SDL3
                {
                    [global::System.Runtime.CompilerServices.SkipLocalsInitAttribute]
                    internal static partial void Log(char* message)
                    {
                        unsafe
                        {
                            __PInvoke(message);
                        }

                        [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_Log", ExactSpelling = true)]
                        static extern unsafe void __PInvoke(char* __message_native);
                    }
                }
            }
            """;

        Markup source = Markup.Parse(InteropSources.SDL("""
                [LibraryImport("SDL3", EntryPoint = "SDL_Log")]
                internal static partial void Log({|KHI011:char*|} message);
            """), generated);

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task CharInWin32ShouldReportNothing()
    {
        const string source = """
            using System;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal delegate void WNDENUMPROC(char* title);

            internal static partial class User32
            {
                [LibraryImport("user32", EntryPoint = "SetWindowTextW")]
                internal static partial int SetWindowText(void* hWnd, char* lpString);

                internal static int Length(ReadOnlySpan<char> text)
                {
                    return Count(text[0]);

                    static int Count(char first) => first;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task ByteStringAndConstInSDLShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_TextInputEvent
            {
                internal uint type;
                internal byte* text;
                internal fixed byte name[32];
                internal const char Separator = '/';

                internal readonly string Name() => "text";
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    [Arguments("KappaDuck.Khaos.Input")]
    [Arguments("KappaDuck.Khaos.Interop")]
    [Arguments("KappaDuck.Khaos.Interop.Browser")]
    public async Task CharOutsideTheSDLAreaShouldReportNothing(string @namespace)
    {
        string source = $$"""
            namespace {{@namespace}};

            internal delegate void Typed(char character);

            internal struct TextInput
            {
                internal char character;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }
}
