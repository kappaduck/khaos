// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

public sealed class NativeIntegerAnalyzerTests
{
    [Test]
    [Arguments("nint")]
    [Arguments("System.IntPtr")]
    public async Task InteropWithNintParameterReportsKHI002(string type)
    {
        string source = $$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow({{type}} window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,48): error KHI002: Parameter 'window' of 'DestroyWindow' is declared as nint; name the native type it carries, such as SDL_Window*");
    }

    [Test]
    [Arguments("nint")]
    [Arguments("System.IntPtr")]
    public async Task InteropReturnNintReportsKHI002(string type)
    {
        string source = $$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateWindow")]
                internal static partial {{type}} CreateWindow();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,29): error KHI002: The return value of 'CreateWindow' is declared as nint; name the native type it carries, such as SDL_Window*");
    }

    [Test]
    public async Task InteropWithoutNintParameterReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_Window;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow(SDL_Window* window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task InteropReturnNoNintReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_Window;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateWindow")]
                internal static partial SDL_Window* CreateWindow();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task Win32InteropWithNintParameterReportsKHI002()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static partial class User32
            {
                [LibraryImport(nameof(User32), EntryPoint = "GetWindowTextLengthW")]
                internal static partial int GetWindowTextLength(nint window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,53): error KHI002: Parameter 'window' of 'GetWindowTextLength' is declared as nint; name the native type it carries, such as HWND*");
    }

    [Test]
    public async Task InteropHelperWithoutLibraryImportReportsKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                internal static bool IsNull(nint handle) => handle == 0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,33): error KHI002: Parameter 'handle' of 'IsNull' is declared as nint; name the native type it carries, such as SDL_Window*");
    }

    [Test]
    public async Task InteropWithGeneratedImplementationReportsKHI002Once()
    {
        const string source = """
            using System.CodeDom.Compiler;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow(nint window);

                [GeneratedCode("Microsoft.Interop.LibraryImportGenerator", "11.0.0")]
                internal static partial void DestroyWindow(nint window) => throw new System.NotImplementedException();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(9,48): error KHI002: Parameter 'window' of 'DestroyWindow' is declared as nint; name the native type it carries, such as SDL_Window*");
    }

    [Test]
    public async Task InteropWithTwoNintParametersReportsKHI002Twice()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetWindowParent")]
                internal static partial void SetWindowParent(nint window, nint parent);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002, KHI002");
    }

    [Test]
    public async Task InteropWithNuintReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_IOStream;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ReadIO")]
                internal static partial nuint ReadIO(SDL_IOStream* context, void* ptr, nuint size);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task NintOutsideAnInteropAreaReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                public nint GetNativeHandle() => 0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
    [Test]
    public async Task InteropStructWithNintFieldReportsKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Surface
            {
                internal nint pixels;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI002: Field 'SDL_Surface.pixels' is declared as nint; name the native type it carries, such as SDL_Window*");
    }

    [Test]
    public async Task InteropStaticNintFieldReportsKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                private static nint _window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,20): error KHI002: Field 'SDL3._window' is declared as nint; name the native type it carries, such as SDL_Window*");
    }

    [Test]
    public async Task InteropStructWithTwoNintFieldsReportsKHI002Twice()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Surface
            {
                internal nint pixels, reserved;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002, KHI002");
    }

    [Test]
    public async Task Win32StructWithNintFieldReportsKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct MENUITEMINFOW
            {
                internal nint hSubMenu;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI002: Field 'MENUITEMINFOW.hSubMenu' is declared as nint; name the native type it carries, such as HWND*");
    }

    [Test]
    public async Task InteropStructWithNuintFieldReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_IOStreamInterface
            {
                internal nuint size;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task InteropStructWithVoidPointerFieldReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Surface
            {
                internal void* pixels;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task NintFieldOutsideAnInteropAreaReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal sealed class Window
            {
                private nint _handle;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<NativeIntegerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
