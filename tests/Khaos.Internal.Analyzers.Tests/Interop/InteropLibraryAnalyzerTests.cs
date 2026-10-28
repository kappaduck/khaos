// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropLibraryAnalyzerTests
{
    [Test]
    public async Task Win32LibraryInSDLAreaShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("user32", EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'user32' does not belong to SDL; it belongs to Win32");
    }

    [Test]
    public async Task SDLLibraryInWin32AreaShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'SDL3' does not belong to Win32; it belongs to SDL");
    }

    [Test]
    public async Task UnknownLibraryInWin32AreaShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class Dwmapi
            {
                [LibraryImport("dwmapi", EntryPoint = "DwmSetWindowAttribute")]
                internal static partial int SetWindowAttribute();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'dwmapi' does not belong to Win32; no interop area declares it");
    }

    [Test]
    public async Task LibraryWithTheDllExtensionShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("user32.dll", EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor(int show);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'user32.dll' does not belong to Win32; no interop area declares it");
    }

    [Test]
    public async Task LibraryNamedThroughNameofInTheWrongAreaShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class User32
            {
                [LibraryImport(nameof(User32), EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'User32' does not belong to SDL; it belongs to Win32");
    }

    [Test]
    public async Task LibraryNamedThroughAConstantInTheWrongAreaShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                private const string Library = "kernel32";

                [LibraryImport(Library, EntryPoint = "GetTickCount64")]
                internal static partial ulong GetTickCount();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().Contain("'kernel32' does not belong to SDL; it belongs to Win32");
    }

    [Test]
    public async Task LibraryInASubNamespaceOfTheAreaShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL.Video;

            internal static partial class SDL3
            {
                [LibraryImport("gdi32", EntryPoint = "GetPixel")]
                internal static partial uint GetPixel();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().Contain("'gdi32' does not belong to SDL; it belongs to Win32");
    }

    [Test]
    public async Task LibraryImportInANestedTypeShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class Native
            {
                internal static partial class Dwm
                {
                    [LibraryImport("dwmapi", EntryPoint = "DwmFlush")]
                    internal static partial int Flush();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
    }

    [Test]
    public async Task DeclarationWithItsGeneratedHalfShouldReportKHI006Once()
    {
        const string declaration = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_Quit")]
                internal static unsafe partial void Quit();
            }
            """;

        const string generated = """
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.Win32
            {
                internal static partial class User32
                {
                    [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_Quit", ExactSpelling = true)]
                    internal static unsafe extern partial void Quit();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(declaration, generated);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().Contain("Source0.cs(7,20)");
    }

    [Test]
    [Arguments("SDL3")]
    [Arguments("SDL3_image")]
    [Arguments("SDL3_ttf")]
    [Arguments("SDL3_mixer")]
    public async Task SDLLibrariesInSDLAreaShouldReportNothing(string library)
    {
        string source = $$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("{{library}}", EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    [Arguments("user32")]
    [Arguments("kernel32")]
    [Arguments("gdi32")]
    public async Task Win32LibrariesInWin32AreaShouldReportNothing(string library)
    {
        string source = $$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class Native
            {
                [LibraryImport("{{library}}", EntryPoint = "GetLastError")]
                internal static partial uint GetLastError();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryNamedThroughNameofShouldReportNothing()
    {
        const string sdl = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            }
            """;

        const string win32 = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport(nameof(User32), EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor(int show);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(sdl, win32);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryImportOutsideInteropShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Windowing;

            internal static partial class Native
            {
                [LibraryImport("dwmapi", EntryPoint = "DwmFlush")]
                internal static partial int Flush();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryImportInAnUnlistedInteropAreaShouldReportNothing()
    {
        const string browser = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Browser;

            internal static partial class Native
            {
                [LibraryImport("user32", EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor(int show);
            }
            """;

        const string root = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop;

            internal static partial class Native
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(browser, root);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryImportInAnotherProductsInteropShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace Fake.Khaos.Interop.SDL;

            internal static partial class User32
            {
                [LibraryImport("user32", EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor(int show);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task MethodWithoutLibraryImportInSDLAreaShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class SDL3
            {
                internal static string Library() => "user32";
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }
}
