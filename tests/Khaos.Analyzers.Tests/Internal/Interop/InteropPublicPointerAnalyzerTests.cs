// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

internal sealed class InteropPublicPointerAnalyzerTests
{
    [Test]
    public async Task PublicPointerPropertyShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                public void* Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,18): error KHI008: 'Window.Handle' exposes a raw pointer; the pointer stays behind the wrapper");
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
        await result.Reported.Should().BeEqualTo("Source0.cs(5,17): error KHI008: 'Window.Handle' exposes nint; wrap the native handle in a type that represents it");
    }

    [Test]
    public async Task PublicConstructorWithPointerShouldReportKHI008ByTheTypeName()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed class Texture
            {
                public Texture(byte* pixels)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,12): error KHI008: 'Texture' exposes a raw pointer; the pointer stays behind the wrapper");
    }

    [Test]
    public async Task PublicDelegateWithPointerShouldReportKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public delegate void PixelCallback(byte* pixels);
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(3,22): error KHI008: 'PixelCallback' exposes a raw pointer; the pointer stays behind the wrapper");
    }

    [Test]
    public async Task PointerAndNintInOneSignatureShouldReportKHI008AsAPointer()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed class Texture
            {
                public void Blit(nint handle, byte* pixels)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,17): error KHI008: 'Texture.Blit' exposes a raw pointer; the pointer stays behind the wrapper");
    }

    [Test]
    [Arguments("public delegate* unmanaged<int, void> {|KHI008:Callback|};")]
    [Arguments("protected byte* {|KHI008:pixels|};")]
    [Arguments("public byte* {|KHI008:Lock|}() => null;")]
    [Arguments("public void {|KHI008:Lock|}(out byte* pixels) => pixels = null;")]
    [Arguments("public void {|KHI008:Write|}(ref byte* cursor) { }")]
    [Arguments("public void {|KHI008:Read|}(in byte* cursor) { }")]
    [Arguments("public byte*[] {|KHI008:Rows|} { get; } = [];")]
    [Arguments("public int {|KHI008:this|}[int* cursor] => 0;")]
    [Arguments("public static implicit operator {|KHI008:void*|}(Texture texture) => null;")]
    [Arguments("public static Texture operator {|KHI008:+|}(Texture texture, byte* pixels) => texture;")]
    [Arguments("public delegate void {|KHI008:Filled|}(byte* pixels);")]
    public async Task PointerInAPublicMemberShouldReportKHI008(string member)
    {
        Markup source = Markup.Parse($$"""
            namespace KappaDuck.Khaos.Graphics;

            public class Texture
            {
                {{member}}
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("public static Window {|KHI008:FromHandle|}(IntPtr handle) => new();")]
    [Arguments("protected internal nint {|KHI008:handle|};")]
    [Arguments("public nint[] {|KHI008:Handles|} { get; } = [];")]
    [Arguments("public IReadOnlyList<nint> {|KHI008:Handles|} { get; } = [];")]
    [Arguments("public Dictionary<int, List<nint>> {|KHI008:Groups|} { get; } = [];")]
    [Arguments("public nint? {|KHI008:Find|}(int index) => null;")]
    [Arguments("public (nint Handle, int Count) {|KHI008:Pair|} => default;")]
    [Arguments("public ref nint {|KHI008:Slot|} => ref Unsafe.NullRef<nint>();")]
    [Arguments("public int {|KHI008:this|}[nint handle] => 0;")]
    [Arguments("public event Action<nint>? {|KHI008:Resized|};")]
    [Arguments("public event Action<nint> {|KHI008:Moved|} { add { } remove { } }")]
    [Arguments("public delegate void {|KHI008:Closed|}(nint handle);")]
    public async Task NintInAPublicMemberShouldReportKHI008(string member)
    {
        Markup source = Markup.Parse($$"""
            using System;
            using System.Collections.Generic;
            using System.Runtime.CompilerServices;

            namespace KappaDuck.Khaos.Windowing;

            public class Window
            {
                {{member}}
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicPropertyShouldReportKHI008OnceNotPerAccessor()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Graphics;

            public sealed class Surface
            {
                public byte* {|KHI008:Pixels|} { get; set; }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicPointerInAFileMarkedGeneratedShouldReportKHI008()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            namespace KappaDuck.Khaos.Windowing
            {
                public sealed partial class Window
                {
                    public nint {|KHI008:Handle|} { get; }
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task InternalEventWithNintShouldReportNothing()
    {
        const string source = """
            using System;

            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                internal event Action<nint>? Resized;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task InternalDelegateWithPointerShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal delegate void SDL_AudioStreamCallback(void* userdata, nint stream);

            public sealed class Callbacks
            {
                internal delegate void SDL_Hook(byte* data);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
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

            public sealed class Window
            {
                private SDL_Window* _window;

                public WindowHandle Handle => default;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PublicMemberOfAnInternalTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            internal sealed class Surface
            {
                public byte* Pixels { get; }

                public nint Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task InternalMemberOfAPublicTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Graphics;

            public sealed class Surface
            {
                internal byte* Pixels { get; }

                private protected nint handle;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
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
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PublicConstNintShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                public const nint Invalid = -1;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropPublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
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
        await result.Located.Should().BeEmpty();
    }
}
