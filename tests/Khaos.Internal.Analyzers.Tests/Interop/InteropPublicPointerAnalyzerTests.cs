// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropPublicPointerAnalyzerTests
{
    [Test]
    public async Task PublicPointerPropertyShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed unsafe class Window
            {
                public void* Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,18): error KHI008: 'Window.Handle' exposes a raw pointer; the pointer stays behind the wrapper");
    }

    [Test]
    public async Task PublicFunctionPointerFieldShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Audio;

            public sealed unsafe class Stream
            {
                public delegate* unmanaged<int, void> Callback;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Stream.Callback' exposes a raw pointer");
    }

    [Test]
    public async Task PublicMethodReturningPointerShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Texture
            {
                public byte* Lock() => null;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Texture.Lock' exposes a raw pointer");
    }

    [Test]
    public async Task PublicOutPointerParameterShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Texture
            {
                public void Lock(out byte* pixels) => pixels = null;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Texture.Lock' exposes a raw pointer");
    }

    [Test]
    public async Task PublicPointerArrayShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Surface
            {
                public byte*[] Rows { get; } = [];
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Surface.Rows' exposes a raw pointer");
    }

    [Test]
    public async Task PublicConstructorWithPointerShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Texture
            {
                public Texture(byte* pixels)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Texture' exposes a raw pointer");
    }

    [Test]
    public async Task PublicIndexerWithPointerParameterShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Surface
            {
                public int this[int* cursor] => 0;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Surface.this[]' exposes a raw pointer");
    }

    [Test]
    public async Task PublicConversionToPointerShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Texture
            {
                public static implicit operator void*(Texture texture) => null;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Texture.op_Implicit' exposes a raw pointer");
    }

    [Test]
    public async Task PublicDelegateWithPointerShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public unsafe delegate void PixelCallback(byte* pixels);
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'PixelCallback' exposes a raw pointer");
    }

    [Test]
    public async Task ProtectedPointerInAPublicClassShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public unsafe class Renderer
            {
                protected byte* pixels;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Renderer.pixels' exposes a raw pointer");
    }

    [Test]
    public async Task PublicNintPropertyShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                public nint Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,17): error KHI008: 'Window.Handle' exposes nint; wrap the native handle in a type that represents it");
    }

    [Test]
    public async Task PublicIntPtrParameterShouldReportKHI008()
    {
        const string source = """
            using System;

            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                public static Window FromHandle(IntPtr handle) => new();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().Contain("'Window.FromHandle' exposes nint");
    }

    [Test]
    public async Task PublicNintInAGenericArgumentShouldReportKHI008()
    {
        const string source = """
            using System.Collections.Generic;

            namespace KappaDuck.Khaos.Windowing;

            public sealed class Displays
            {
                public IReadOnlyList<nint> Handles { get; } = [];

                public nint? Find(int index) => null;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008, KHI008");
        await result.Reported.Should().Contain("'Displays.Handles' exposes nint");
        await result.Reported.Should().Contain("'Displays.Find' exposes nint");
    }

    [Test]
    public async Task PublicPropertyShouldReportKHI008OnceNotPerAccessor()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Surface
            {
                public byte* Pixels { get; set; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
    }

    [Test]
    public async Task PointerBehindAWrapperShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal readonly struct SDL_Window;

            public readonly struct WindowHandle
            {
            }

            public sealed unsafe class Window
            {
                private SDL_Window* _window;

                public WindowHandle Handle => default;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task PublicMemberOfAnInternalTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            internal sealed unsafe class Surface
            {
                public byte* Pixels { get; }

                public nint Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task InternalMemberOfAPublicTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed unsafe class Surface
            {
                internal byte* Pixels { get; }

                private protected nint handle;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task PublicNuintShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Audio;

            public sealed class Buffer
            {
                public nuint Size { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task SuppressedWindowHandleShouldReportNothing()
    {
        const string source = """
            using System.Diagnostics.CodeAnalysis;

            namespace KappaDuck.Khaos.Windowing;

            public readonly struct WindowHandle
            {
                [SuppressMessage("Interop", "KHI008", Justification = "The one native handle Khaos hands out, for overlays and video players.")]
                public nint Hwnd { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }
}
