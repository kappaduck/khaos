// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropBoolMarshallingAnalyzerTests
{
    [Test]
    public async Task BoolReturnWithoutMarshalAsInSDLAreaShouldReportKHI001()
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

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,29): error KHI001: The return value of 'HasClipboardText' does not follow the SDL bool convention; expected [return: MarshalAs(UnmanagedType.U1)]");
    }

    [Test]
    public async Task BoolParameterWithoutMarshalAsInSDLAreaShouldReportKHI001()
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

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,58): error KHI001: Parameter 'enabled' of 'SetGamepadEventsEnabled' does not follow the SDL bool convention; expected [MarshalAs(UnmanagedType.U1)]");
    }

    [Test]
    public async Task BoolParameterMarshalledAsU1InWin32AreaShouldReportKHI001()
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

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,74): error KHI001: Parameter 'show' of 'ShowCursor' does not follow the Win32 bool convention; expected [MarshalAs(UnmanagedType.Bool)]");
    }

    [Test]
    public async Task BoolReturnMarshalledAsBoolInSDLAreaShouldReportKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HasClipboardText")]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().Contain("expected [return: MarshalAs(UnmanagedType.U1)]");
    }

    [Test]
    public async Task BoolMarshalledAsI1InSDLAreaShouldReportKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetGamepadEventsEnabled")]
                internal static partial void SetGamepadEventsEnabled([MarshalAs(UnmanagedType.I1)] bool enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
    }

    [Test]
    public async Task BoolMarshalledAsI4InWin32AreaShouldReportKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("user32", EntryPoint = "IsWindowVisible")]
                [return: MarshalAs(UnmanagedType.I4)]
                internal static partial bool IsWindowVisible(nuint window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().Contain("expected [return: MarshalAs(UnmanagedType.Bool)]");
    }

    [Test]
    public async Task BoolMarshalledThroughTheShortConstructorShouldReportKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetGamepadEventsEnabled")]
                internal static partial void SetGamepadEventsEnabled([MarshalAs((short)UnmanagedType.Bool)] bool enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
    }

    [Test]
    [Arguments("ref")]
    [Arguments("out")]
    [Arguments("in")]
    public async Task BoolPassedByReferenceWithoutTheConventionShouldReportKHI001(string modifier)
    {
        string source = $$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("user32", EntryPoint = "GetWindowEnabled")]
                internal static partial int GetWindowEnabled([MarshalAs(UnmanagedType.U1)] {{modifier}} bool enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().Contain("Parameter 'enabled' of 'GetWindowEnabled' does not follow the Win32 bool convention");
    }

    [Test]
    public async Task EveryBoolOfAMethodShouldReportKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Fake")]
                internal static partial bool Fake(bool first, int count, [MarshalAs(UnmanagedType.U1)] bool second, bool third);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001, KHI001, KHI001");
        await result.Reported.Should().Contain("The return value of 'Fake'");
        await result.Reported.Should().Contain("Parameter 'first' of 'Fake'");
        await result.Reported.Should().Contain("Parameter 'third' of 'Fake'");
    }

    [Test]
    public async Task BoolInASubNamespaceOfTheAreaShouldReportKHI001()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL.Video;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ScreenSaverEnabled")]
                internal static partial bool ScreenSaverEnabled();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
    }

    [Test]
    public async Task BoolInAFileMarkedGeneratedShouldReportKHI001()
    {
        const string source = """
            // <auto-generated/>
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_HasClipboardText")]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
    }

    [Test]
    public async Task DeclarationWithItsGeneratedHalfShouldReportKHI001Once()
    {
        const string declaration = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetGamepadEventsEnabled")]
                internal static partial void SetGamepadEventsEnabled([MarshalAs(UnmanagedType.Bool)] bool enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(declaration, GeneratedSetGamepadEventsEnabled);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI001");
        await result.Reported.Should().Contain("Source0.cs(8,90)");
    }

    [Test]
    public async Task BoolsFollowingTheSDLConventionShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Fake")]
                [return: MarshalAs(UnmanagedType.U1)]
                internal static partial bool Fake([MarshalAs(UnmanagedType.U1)] bool first, [MarshalAs(UnmanagedType.U1)] out bool second);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task BoolsFollowingTheWin32ConventionShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("user32", EntryPoint = "Fake")]
                [return: MarshalAs(UnmanagedType.Bool)]
                internal static partial bool Fake([MarshalAs(UnmanagedType.Bool)] bool first, [MarshalAs(UnmanagedType.Bool)] ref bool second);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task CorrectDeclarationWithItsGeneratedHalfShouldReportNothing()
    {
        const string declaration = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetGamepadEventsEnabled")]
                internal static partial void SetGamepadEventsEnabled([MarshalAs(UnmanagedType.U1)] bool enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(declaration, GeneratedSetGamepadEventsEnabled);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task BoolPointerShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static unsafe partial class User32
            {
                [LibraryImport("user32", EntryPoint = "GetWindowEnabled")]
                internal static partial int GetWindowEnabled(bool* enabled);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task BoolOutsideInteropShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Windowing;

            internal static partial class Native
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_HasClipboardText")]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task BoolInAnUnlistedInteropAreaShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Browser;

            internal static partial class Native
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_HasClipboardText")]
                internal static partial bool HasClipboardText();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task MethodWithoutLibraryImportShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Conversions
            {
                internal static bool IsSet(bool value) => value;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropBoolMarshallingAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    private const string GeneratedSetGamepadEventsEnabled = """
        // <auto-generated/>
        namespace KappaDuck.Khaos.Interop.SDL
        {
            internal static unsafe partial class SDL3
            {
                [global::System.Runtime.CompilerServices.SkipLocalsInitAttribute]
                internal static partial void SetGamepadEventsEnabled(bool enabled)
                {
                    byte __enabled_native = (byte)(enabled ? 1 : 0);
                    unsafe
                    {
                        __PInvoke(__enabled_native);
                    }

                    [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_SetGamepadEventsEnabled", ExactSpelling = true)]
                    static extern unsafe void __PInvoke(byte __enabled_native);
                }
            }
        }
        """;
}
