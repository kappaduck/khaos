// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

public sealed class NativeLibraryAnalyzerTests
{
    [Test]
    public async Task Win32LibraryInSDLAreaReportsKHI006()
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

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'user32' does not belong to SDL; it belongs to Win32");
    }

    [Test]
    public async Task SDLLibraryInWin32AreaReportsKHI006()
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

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'SDL3' does not belong to Win32; it belongs to SDL");
    }

    [Test]
    public async Task UnknownLibraryReportsKHI006()
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

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'dwmapi' does not belong to Win32; no interop area declares it");
    }

    [Test]
    public async Task GeneratedImplementationReportsKHI006Once()
    {
        const string source = """
            using System.CodeDom.Compiler;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("user32", EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();

                [GeneratedCode("Microsoft.Interop.LibraryImportGenerator", "11.0.0")]
                internal static partial int GetWindowTextLength() => 0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI006");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,20): error KHI006: 'user32' does not belong to SDL; it belongs to Win32");
    }

    [Test]
    public async Task SatelliteLibraryFromNameofReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3_image
            {
                [LibraryImport(nameof(SDL3_image), EntryPoint = "IMG_Version")]
                internal static partial int Version();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task Win32LibraryFromNameofReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport(nameof(User32), EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryOutsideAnInteropAreaReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Platform;

            internal static partial class Native
            {
                [LibraryImport("dwmapi", EntryPoint = "DwmFlush")]
                internal static partial int Flush();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
