// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

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
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'user32' does not belong to SDL; it belongs to Win32");
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
        await result.Reported.Should().BeEqualTo("Source0.cs(7,20): error KHI006: 'dwmapi' does not belong to Win32; no interop area declares it");
    }

    [Test]
    public async Task EntryPointOfAnotherSDLLibraryShouldReportKHI006()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "IMG_LoadTexture")]
                internal static partial void* LoadTexture(void* renderer, byte* file);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(7,28): error KHI006: 'IMG_LoadTexture' is not a function of 'SDL3', whose functions start with SDL_");
    }

    [Test]
    [Arguments("user32")]
    [Arguments("kernel32")]
    [Arguments("gdi32")]
    public async Task Win32LibrariesInSDLAreaShouldReportKHI006(string library)
    {
        Markup source = Markup.Parse(InteropSources.SDL($$"""
                [LibraryImport({|KHI006:"{{library}}"|}, EntryPoint = "GetLastError")]
                internal static partial uint GetLastError();
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("SDL3")]
    [Arguments("SDL3_image")]
    [Arguments("SDL3_ttf")]
    [Arguments("SDL3_mixer")]
    public async Task SDLLibrariesInWin32AreaShouldReportKHI006(string library)
    {
        Markup source = Markup.Parse(InteropSources.Win32($$"""
                [LibraryImport({|KHI006:"{{library}}"|}, EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("dwmapi")]
    [Arguments("SDL3.dll")]
    [Arguments("libSDL3.so")]
    public async Task UnknownLibraryInSDLAreaShouldReportKHI006(string library)
    {
        Markup source = Markup.Parse(InteropSources.SDL($$"""
                [LibraryImport({|KHI006:"{{library}}"|}, EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task LibraryWithTheDllExtensionInWin32AreaShouldReportKHI006()
    {
        Markup source = Markup.Parse(InteropSources.Win32("""
                [LibraryImport({|KHI006:"user32.dll"|}, EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor(int show);
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task LibraryNamedThroughNameofInTheWrongAreaShouldReportKHI006()
    {
        Markup source = Markup.Parse("""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class User32
            {
                [LibraryImport({|KHI006:nameof(User32)|}, EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task LibraryNamedThroughAConstantInTheWrongAreaShouldReportKHI006()
    {
        Markup source = Markup.Parse(InteropSources.SDL("""
                private const string Library = "kernel32";

                [LibraryImport({|KHI006:Library|}, EntryPoint = "GetTickCount64")]
                internal static partial ulong GetTickCount();
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task LibraryInASubNamespaceOfTheAreaShouldReportKHI006()
    {
        Markup source = Markup.Parse("""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL.Video;

            internal static partial class SDL3
            {
                [LibraryImport({|KHI006:"gdi32"|}, EntryPoint = "GetPixel")]
                internal static partial uint GetPixel();
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task LibraryImportInANestedTypeShouldReportKHI006()
    {
        Markup source = Markup.Parse("""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class Native
            {
                internal static partial class Dwm
                {
                    [LibraryImport({|KHI006:"dwmapi"|}, EntryPoint = "DwmFlush")]
                    internal static partial int Flush();
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task WrongLibraryInAFileMarkedGeneratedShouldReportKHI006()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport({|KHI006:"user32"|}, EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task DeclarationWithItsGeneratedHalfShouldReportKHI006Once()
    {
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

        Markup source = Markup.Parse(InteropSources.Win32("""
                [LibraryImport({|KHI006:"SDL3"|}, EntryPoint = "SDL_Quit")]
                internal static unsafe partial void Quit();
            """), generated);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("SDL3", "IMG_Load")]
    [Arguments("SDL3", "sdl_Quit")]
    [Arguments("SDL3_image", "SDL_Quit")]
    [Arguments("SDL3_ttf", "MIX_Init")]
    [Arguments("SDL3_mixer", "TTF_Init")]
    public async Task EntryPointWithoutThePrefixOfItsLibraryShouldReportKHI006(string library, string entryPoint)
    {
        Markup source = Markup.Parse(InteropSources.SDL($$"""
                [LibraryImport("{{library}}", {|KHI006:EntryPoint = "{{entryPoint}}"|})]
                internal static partial void Call();
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task EntryPointAmongOtherNamedArgumentsShouldReportKHI006AtTheEntryPoint()
    {
        Markup source = Markup.Parse(InteropSources.SDL("""
                [LibraryImport("SDL3", StringMarshalling = StringMarshalling.Utf8, {|KHI006:EntryPoint = "IMG_Load"|}, SetLastError = true)]
                internal static partial void* Load(string file);
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task LibraryInTheWrongAreaShouldReportKHI006OnceAtTheLibrary()
    {
        Markup source = Markup.Parse(InteropSources.Win32("""
                [LibraryImport({|KHI006:"SDL3_image"|}, EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("SDL3", "SDL_GetVersion")]
    [Arguments("SDL3_image", "IMG_Version")]
    [Arguments("SDL3_ttf", "TTF_Version")]
    [Arguments("SDL3_mixer", "MIX_Version")]
    public async Task SDLLibrariesInSDLAreaShouldReportNothing(string library, string entryPoint)
    {
        string source = InteropSources.SDL($$"""
                [LibraryImport("{{library}}", EntryPoint = "{{entryPoint}}")]
                internal static partial int Version();
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    [Arguments("user32")]
    [Arguments("kernel32")]
    [Arguments("gdi32")]
    public async Task Win32LibrariesInWin32AreaShouldReportNothing(string library)
    {
        string source = InteropSources.Win32($$"""
                [LibraryImport("{{library}}", EntryPoint = "GetLastError")]
                internal static partial uint GetLastError();
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    [Arguments("")]
    [Arguments(", EntryPoint = \"\"")]
    [Arguments(", EntryPoint = \"   \"")]
    [Arguments(", EntryPoint = null")]
    public async Task SDLImportWithoutAnEntryPointShouldLeaveItToKHI003(string entryPoint)
    {
        string source = InteropSources.SDL($$"""
                [LibraryImport(nameof(SDL3){{entryPoint}})]
                internal static partial void Quit();
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryNamedThroughNameofShouldReportNothing()
    {
        string sdl = InteropSources.SDL("""
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            """);

        string win32 = InteropSources.Win32("""
                [LibraryImport(nameof(User32), EntryPoint = "ShowCursor")]
                internal static partial int ShowCursor(int show);
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(sdl, win32);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryInAnotherCaseShouldReportNothing()
    {
        string sdl = InteropSources.SDL("""
                [LibraryImport("sdl3_IMAGE", EntryPoint = "IMG_Version")]
                internal static partial int Version();
            """);

        string win32 = InteropSources.Win32("""
                [LibraryImport("KERNEL32", EntryPoint = "GetLastError")]
                internal static partial uint GetLastError();
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(sdl, win32);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
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
        await result.Located.Should().BeEmpty();
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
        await result.Located.Should().BeEmpty();
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
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task MethodWithoutLibraryImportInSDLAreaShouldReportNothing()
    {
        string source = InteropSources.SDL("""
                internal static string Library() => "user32";
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task CompilationWithoutLibraryImportShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [System.Runtime.InteropServices.LibraryImport("user32", EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropLibraryAnalyzer>.RunAsync(TestCompilation.WithoutReferences(source));

        await result.Located.Should().BeEmpty();
    }
}
