// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropSDLCharAnalyzerTests
{
    [Test]
    public async Task CharPointerFieldInSDLStructShouldReportKHI011()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_TextInputEvent
            {
                internal uint type;
                internal char* text;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI011");
        await result.Reported.Should().BeEqualTo("Source0.cs(6,14): error KHI011: Field 'SDL_TextInputEvent.text' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CharParameterShouldReportKHI011()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_Window;

            internal static unsafe partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_SetWindowTitle")]
                internal static partial byte SetWindowTitle(SDL_Window* window, char* title);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI011");
        await result.Reported.Should().Contain("Parameter 'title' of 'SetWindowTitle' uses char; a C char is 1 byte in SDL, store it as byte");
    }

    [Test]
    public async Task CharReturnShouldReportKHI011()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_GetError")]
                internal static partial char* GetError();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI011");
        await result.Reported.Should().Contain("The return value of 'GetError' uses char");
    }

    [Test]
    public async Task FixedCharBufferShouldReportKHI011()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_DisplayName
            {
                internal fixed char name[32];
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI011");
        await result.Reported.Should().Contain("Field 'SDL_DisplayName.name' uses char");
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
        await result.Ids.Should().BeEqualTo("KHI011");
        await result.Reported.Should().Contain("Property 'SDL_Key.Character' uses char");
    }

    [Test]
    public async Task FunctionPointerWithCharShouldReportKHI011()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Callbacks
            {
                internal delegate* unmanaged<char*, void> log;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI011");
        await result.Reported.Should().Contain("Field 'SDL_Callbacks.log' uses char");
    }

    [Test]
    public async Task CharInWin32ShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static unsafe partial class User32
            {
                [LibraryImport("user32", EntryPoint = "SetWindowTextW")]
                internal static partial int SetWindowText(void* hWnd, char* lpString);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task ByteInSDLShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_TextInputEvent
            {
                internal uint type;
                internal byte* text;
                internal fixed byte name[32];
                internal const char Separator = '/';
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task CharOutsideInteropShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            internal struct TextInput
            {
                internal char character;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropSDLCharAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }
}
