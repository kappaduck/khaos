// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

public sealed class BooleanMarshallingAnalyzerTests
{
    [Test]
    public async Task SDLReturnWithoutMarshallingReportsKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HasClipboardText")]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,29): error KHI001: The return value of 'HasClipboardText' does not follow the SDL bool convention; expected [return: MarshalAs(UnmanagedType.U1)]");
    }

    [Test]
    public async Task SDLReturnWithU1ReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HasClipboardText")]
                [return: MarshalAs(UnmanagedType.U1)]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task Win32ReturnWithU1ReportsKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class Kernel32
            {
                [LibraryImport("kernel32", EntryPoint = "Beep")]
                [return: MarshalAs(UnmanagedType.U1)]
                internal static partial bool Beep(uint frequency, uint duration);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(9,29): error KHI001: The return value of 'Beep' does not follow the Win32 bool convention; expected [return: MarshalAs(UnmanagedType.Bool)]");
    }

    [Test]
    public async Task Win32ReturnWithBoolReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class Kernel32
            {
                [LibraryImport("kernel32", EntryPoint = "Beep")]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static partial bool Beep(uint frequency, uint duration);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task BindingOutsideAnInteropAreareportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Platform;

            internal static partial class Native
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_HasClipboardText")]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task SDLReturnWithGeneratedImplementationReportsKHI001()
    {
        const string source = """
            using System.CodeDom.Compiler;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HasClipboardText")]
                internal static partial bool HasClipboardText();

                [GeneratedCode("Microsoft.Interop.LibraryImportGenerator", "11.0.0")]
                internal static partial bool HasClipboardText() => throw new System.NotImplementedException();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(9,29): error KHI001: The return value of 'HasClipboardText' does not follow the SDL bool convention; expected [return: MarshalAs(UnmanagedType.U1)]");
    }

    [Test]
    public async Task SDLParameterWithoutMarshallingReportsKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetGamepadEventsEnabled")]
                internal static partial void SetGamepadEventsEnabled(bool enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,58): error KHI001: Parameter 'enabled' of 'SetGamepadEventsEnabled' does not follow the SDL bool convention; expected [MarshalAs(UnmanagedType.U1)]");
    }

    [Test]
    public async Task Win32ParameterWithU1ReportsKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("user32", EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor([MarshalAs(UnmanagedType.U1)] bool show);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,74): error KHI001: Parameter 'show' of 'ShowCursor' does not follow the Win32 bool convention; expected [MarshalAs(UnmanagedType.Bool)]");
    }

    [Test]
    public async Task BoolFieldInSDLStructReportsKHI009()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal bool down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI009");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI009: Field 'SDL_KeyboardEvent.down' is declared as bool; in SDL a native boolean is stored as byte");
    }

    [Test]
    public async Task AnnotatedBoolFieldInWin32StructStillReportsKHI009()
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

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI009");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,14): error KHI009: Field 'PAINTSTRUCT.fErase' is declared as bool; in Win32 a native boolean is stored as int");
    }

    [Test]
    public async Task NativeWidthAndConstFieldsReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_KeyboardEvent
            {
                internal const bool Tracked = true;

                internal byte down;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task BoolFieldInClassReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal sealed class SdlState
            {
                internal bool Initialized;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<BooleanMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
