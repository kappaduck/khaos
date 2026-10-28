// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropNintAnalyzerTests
{
    [Test]
    public async Task NintReturnInSDLShouldReportKHI002()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_GetWindowFromID")]
                internal static partial nint GetWindowFromID(uint id);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,29): error KHI002: The return value of 'GetWindowFromID' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct");
    }

    [Test]
    public async Task NintParameterInSDLShouldReportKHI002()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow(nint window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,48): error KHI002: Parameter 'window' of 'DestroyWindow' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct");
    }

    [Test]
    public async Task IntPtrParameterShouldReportKHI002()
    {
        const string source = """
            using System;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow(IntPtr window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Parameter 'window' of 'DestroyWindow' uses nint");
    }

    [Test]
    public async Task OutNintParametersShouldReportKHI002()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_CreateWindowAndRenderer")]
                internal static partial byte CreateWindowAndRenderer(byte* title, int width, int height, ulong flags, out nint window, out nint renderer);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002, KHI002");
        await result.Reported.Should().Contain("Parameter 'window' of 'CreateWindowAndRenderer' uses nint");
        await result.Reported.Should().Contain("Parameter 'renderer' of 'CreateWindowAndRenderer' uses nint");
    }

    [Test]
    public async Task RefNintReturnShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Slots
            {
                private static readonly nint[] _slots = new nint[4];

                internal static ref nint At(int index) => ref _slots[index];
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("The return value of 'At' uses nint");
    }

    [Test]
    public async Task NintPointerParameterShouldReportKHI002()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_Fake")]
                internal static partial void Fake(nint** handles);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Parameter 'handles' of 'Fake' uses nint");
    }

    [Test]
    public async Task NintInlineArrayShouldReportKHI002()
    {
        const string source = """
            using System.Runtime.CompilerServices;

            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            [InlineArray(4)]
            internal struct SDL_Handles
            {
                private nint _element;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,13): error KHI002: Field 'SDL_Handles._element' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct");
    }

    [Test]
    public async Task FunctionPointerFieldWithNintShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Callbacks
            {
                internal int version;
                internal delegate* unmanaged<nint, void> cleanup;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Field 'SDL_Callbacks.cleanup' uses nint");
    }

    [Test]
    public async Task NintFieldInSDLStructShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Surface
            {
                internal int w;
                internal nint pixels;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(6,14): error KHI002: Field 'SDL_Surface.pixels' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct");
    }

    [Test]
    public async Task NintFieldInWin32StructShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal struct MSG
            {
                internal uint message;
                internal nuint wParam;
                internal nint lParam;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Field 'MSG.lParam' uses nint");
    }

    [Test]
    public async Task StructWithNintAndAnotherFieldShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal readonly struct LPARAM
            {
                internal readonly nint Value;
                internal readonly int Tag;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Field 'LPARAM.Value' uses nint");
    }

    [Test]
    public async Task NintAutoPropertyShouldReportKHI002Once()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal struct SDL_Surface
            {
                internal int w;
                internal nint Pixels { get; set; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().BeEqualTo("Source0.cs(6,14): error KHI002: Property 'SDL_Surface.Pixels' uses nint; type it as the native pointer it carries, or wrap a pointer-sized integer in its own struct");
    }

    [Test]
    public async Task NintComputedPropertyShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Surface
            {
                internal int w;
                internal void* pixels;

                internal readonly nint Address => (nint)pixels;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Property 'SDL_Surface.Address' uses nint");
    }

    [Test]
    public async Task StaticNintFieldInAClassShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Handles
            {
                internal static nint window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Field 'Handles.window' uses nint");
    }

    [Test]
    public async Task UnmanagedCallersOnlyCallbackWithNintShouldReportKHI002()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static class WindowProcedure
            {
                [UnmanagedCallersOnly]
                internal static nint Handle(nint hwnd, uint message, nuint wParam, nint lParam) => 0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002, KHI002, KHI002");
        await result.Reported.Should().Contain("The return value of 'Handle' uses nint");
        await result.Reported.Should().Contain("Parameter 'hwnd' of 'Handle' uses nint");
        await result.Reported.Should().Contain("Parameter 'lParam' of 'Handle' uses nint");
    }

    [Test]
    public async Task NintConstructorParameterOutsideAWrapperShouldReportKHI002()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Surface
            {
                internal int w;
                internal void* pixels;

                internal SDL_Surface(int w, nint pixels)
                {
                    this.w = w;
                    this.pixels = (void*)pixels;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Parameter 'pixels' of 'SDL_Surface' uses nint");
    }

    [Test]
    public async Task LibraryImportWithItsGeneratedHalfShouldReportKHI002Once()
    {
        const string declaration = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_GetWindowFromID")]
                internal static partial nint GetWindowFromID(uint id);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(declaration, GeneratedGetWindowFromID);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI002");
        await result.Reported.Should().Contain("Source0.cs(8,29)");
    }

    [Test]
    public async Task NintWrapperStructShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal readonly struct LPARAM
            {
                internal static readonly nint Invalid = -1;

                internal readonly nint Value;

                internal LPARAM(nint value) => Value = value;

                public static implicit operator nint(LPARAM value) => value.Value;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task NintWrapperWithAnAutoPropertyShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal readonly struct LRESULT
            {
                internal LRESULT(nint value) => Value = value;

                internal nint Value { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task WrappersInASignatureShouldReportNothing()
    {
        const string primitives = """
            namespace KappaDuck.Khaos.Interop.Win32.Primitives;

            internal readonly struct HWND
            {
            }

            internal readonly struct LPARAM
            {
                internal readonly nint Value;
            }

            internal readonly struct LRESULT
            {
                internal readonly nint Value;
            }
            """;

        const string user32 = """
            using System.Runtime.InteropServices;
            using KappaDuck.Khaos.Interop.Win32.Primitives;

            namespace KappaDuck.Khaos.Interop.Win32;

            internal static unsafe partial class User32
            {
                [LibraryImport("user32", EntryPoint = "DefWindowProcW")]
                internal static partial LRESULT DefWindowProcW(HWND* hWnd, uint Msg, nuint wParam, LPARAM lParam);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(primitives, user32);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task NuintAndUIntPtrShouldReportNothing()
    {
        const string source = """
            using System;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_IOStream
            {
            }

            internal static unsafe partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_ReadIO")]
                internal static partial nuint ReadIO(SDL_IOStream* context, void* ptr, nuint size);

                [LibraryImport("SDL3", EntryPoint = "SDL_WriteIO")]
                internal static partial UIntPtr WriteIO(SDL_IOStream* context, void* ptr, UIntPtr size);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task TypedPointersShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_Window
            {
            }

            internal unsafe struct SDL_Surface
            {
                internal int w;
                internal void* pixels;
            }

            internal static unsafe partial class SDL3
            {
                [LibraryImport("SDL3", EntryPoint = "SDL_GetWindowFromID")]
                internal static partial SDL_Window* GetWindowFromID(uint id);

                [LibraryImport("SDL3", EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow(SDL_Window* window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task NintLocalsAndExpressionsShouldReportNothing()
    {
        const string source = """
            using System;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe class Userdata
            {
                internal static void* From(object state)
                {
                    nint handle = GCHandle.ToIntPtr(GCHandle.Alloc(state));
                    return handle == IntPtr.Zero || sizeof(nint) == 0 ? null : (void*)handle;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task ConstNintShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32;

            internal static class User32
            {
                internal const nint Sentinel = -1;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task NintArrayAndGenericArgumentShouldReportNothing()
    {
        const string source = """
            using System;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Handles
            {
                private static readonly nint[] _handles = [];

                internal static int Count(ReadOnlySpan<nint> handles) => handles.Length + _handles.Length;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task NintOutsideInteropShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal readonly struct WindowHandle
            {
                internal WindowHandle(nint hwnd, int display)
                {
                    Hwnd = hwnd;
                    Display = display;
                }

                internal nint Hwnd { get; }

                internal int Display { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task NintInAnUnlistedInteropAreaShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Browser;

            internal static class Canvas
            {
                internal static nint context;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNintAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    private const string GeneratedGetWindowFromID = """
        // <auto-generated/>
        namespace KappaDuck.Khaos.Interop.SDL
        {
            internal static unsafe partial class SDL3
            {
                [global::System.Runtime.CompilerServices.SkipLocalsInitAttribute]
                internal static partial nint GetWindowFromID(uint id)
                {
                    nint __retVal;
                    unsafe
                    {
                        __retVal = __PInvoke(id);
                    }

                    return __retVal;

                    [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_GetWindowFromID", ExactSpelling = true)]
                    static extern unsafe nint __PInvoke(uint __id_native);
                }
            }
        }
        """;
}
