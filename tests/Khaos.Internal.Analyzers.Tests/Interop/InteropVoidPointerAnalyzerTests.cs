// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropVoidPointerAnalyzerTests
{
    [Test]
    public async Task VoidPointerParameterWithoutAReasonShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetPointerProperty")]
                internal static partial byte SetPointerProperty(uint props, byte* name, void* value);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,34): error KHI004: 'SetPointerProperty' takes or returns void* without [UntypedBecause]; say why it cannot be typed");
    }

    [Test]
    public async Task VoidPointerReturnWithoutAReasonShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_malloc")]
                internal static partial void* Malloc(nuint size);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'Malloc' takes or returns void*");
    }

    [Test]
    public async Task VoidPointerPointerShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Fake")]
                internal static partial void Fake(void** values);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'Fake' takes or returns void*");
    }

    [Test]
    public async Task OutVoidPointerShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Fake")]
                internal static partial void Fake(out void* value);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'Fake' takes or returns void*");
    }

    [Test]
    public async Task FunctionPointerWithVoidPointerShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Fake")]
                internal static partial void Fake(delegate* unmanaged<void*, void> cleanup);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'Fake' takes or returns void*");
    }

    [Test]
    public async Task UnmanagedCallersOnlyCallbackShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal struct SDL_Event
            {
                internal uint type;
            }

            internal static unsafe class EventFilter
            {
                [UnmanagedCallersOnly]
                internal static byte Filter(void* userdata, SDL_Event* e) => 1;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'Filter' takes or returns void*");
    }

    [Test]
    public async Task HelperReturningVoidPointerShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe class Userdata
            {
                internal static void* From(object state) => (void*)GCHandle.ToIntPtr(GCHandle.Alloc(state));
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'From' takes or returns void*");
    }

    [Test]
    public async Task ConstructorWithVoidPointerShouldReportKHI004()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Surface
            {
                internal int w;
                internal void* pixels;

                internal SDL_Surface(int w, void* pixels)
                {
                    this.w = w;
                    this.pixels = pixels;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'SDL_Surface' takes or returns void*");
    }

    [Test]
    public async Task EmptyReasonShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetPointerProperty")]
                [UntypedBecause("")]
                internal static partial byte SetPointerProperty(uint props, byte* name, void* value);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,21): error KHI004: 'SetPointerProperty' gives [UntypedBecause] an empty reason; say why it cannot be typed");
    }

    [Test]
    public async Task BlankReasonShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetPointerProperty")]
                [UntypedBecause("   ")]
                internal static partial byte SetPointerProperty(uint props, byte* name, void* value);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("gives [UntypedBecause] an empty reason");
    }

    [Test]
    public async Task UntypedBecauseFromAnotherNamespaceShouldReportKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal sealed class UntypedBecauseAttribute(string reason) : System.Attribute
            {
                internal string Reason { get; } = reason;
            }

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_malloc")]
                [UntypedBecause("SDL returns raw memory the caller gives a type to")]
                internal static partial void* Malloc(nuint size);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("'Malloc' takes or returns void* without [UntypedBecause]");
    }

    [Test]
    public async Task VoidPointerWithItsGeneratedHalfShouldReportKHI004Once()
    {
        const string declaration = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_malloc")]
                internal static partial void* Malloc(nuint size);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(declaration, GeneratedMalloc, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().Contain("Source0.cs(8,35)");
    }

    [Test]
    public async Task VoidPointerWithAReasonShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal struct SDL_Event
            {
                internal uint type;
            }

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetEventFilter")]
                [UntypedBecause("SDL hands userdata back to the filter untouched and never reads it")]
                internal static partial void SetEventFilter(delegate* unmanaged<void*, SDL_Event*, byte> filter, void* userdata);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task TypedPointersShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal readonly struct SDL_Window;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_CreateWindow")]
                internal static partial SDL_Window* CreateWindow(byte* title, int w, int h, ulong flags);

                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_DestroyWindow")]
                internal static partial void DestroyWindow(SDL_Window* window);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task VoidReturnShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
                internal static partial void Quit();

                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Fake")]
                internal static partial void Fake(delegate* unmanaged<int, void> callback);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task VoidPointerFieldShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal unsafe struct SDL_Surface
            {
                internal int w;
                internal void* pixels;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task VoidPointerOutsideInteropShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            internal static unsafe class Pixels
            {
                internal static void* Offset(void* pixels, int bytes) => (byte*)pixels + bytes;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task VoidPointerInAnUnlistedInteropAreaShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Browser;

            internal static unsafe class Canvas
            {
                internal static void* Context() => null;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVoidPointerAnalyzer>.RunAsync(source, UntypedBecause);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    private const string UntypedBecause = """
        namespace KappaDuck.Khaos.Interop;

        [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Constructor, Inherited = false)]
        internal sealed class UntypedBecauseAttribute(string reason) : System.Attribute
        {
            internal string Reason { get; } = reason;
        }
        """;

    private const string GeneratedMalloc = """
        // <auto-generated/>
        namespace KappaDuck.Khaos.Interop.SDL
        {
            internal static unsafe partial class SDL3
            {
                [global::System.Runtime.CompilerServices.SkipLocalsInitAttribute]
                internal static partial void* Malloc(nuint size)
                {
                    void* __retVal;
                    unsafe
                    {
                        __retVal = __PInvoke(size);
                    }

                    return __retVal;

                    [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_malloc", ExactSpelling = true)]
                    static extern unsafe void* __PInvoke(nuint __size_native);
                }
            }
        }
        """;
}
