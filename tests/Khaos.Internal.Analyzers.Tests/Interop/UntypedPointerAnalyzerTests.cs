// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

public sealed class UntypedPointerAnalyzerTests
{
    private const string Attribute = """
        namespace KappaDuck.Khaos.Interop;

        [System.AttributeUsage(System.AttributeTargets.Method)]
        internal sealed class UntypedPointerAttribute(string reason) : System.Attribute
        {
            internal string Reason { get; } = reason;
        }
        """;

    [Test]
    public async Task VoidPointerWithoutReasonReportsKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetEventFilter")]
                internal static partial void SetEventFilter(void* userdata);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<UntypedPointerAnalyzer>.RunAsync(source, Attribute);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().BeEqualTo("Source0.cs(8,34): error KHI004: 'SetEventFilter' takes or returns void* without [UntypedPointer]; say why it cannot be typed");
    }

    [Test]
    public async Task VoidPointerWithEmptyReasonReportsKHI004()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetEventFilter")]
                [UntypedPointer("   ")]
                internal static partial void SetEventFilter(void* userdata);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<UntypedPointerAnalyzer>.RunAsync(source, Attribute);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI004");
        await result.Reported.Should().BeEqualTo("Source0.cs(9,34): error KHI004: 'SetEventFilter' takes or returns void* without [UntypedPointer]; say why it cannot be typed");
    }

    [Test]
    public async Task VoidPointerWithReasonReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static unsafe partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_SetEventFilter")]
                [UntypedPointer("SDL hands userdata back to the filter untouched and never reads it")]
                internal static partial void SetEventFilter(void* userdata);
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<UntypedPointerAnalyzer>.RunAsync(source, Attribute);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task TypedPointerReportsNothing()
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

        AnalyzerResult result = await AnalyzerRunner<UntypedPointerAnalyzer>.RunAsync(source, Attribute);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task VoidPointerOutsideAnInteropAreaReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal static unsafe class Native
            {
                internal static void Attach(void* handle)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<UntypedPointerAnalyzer>.RunAsync(source, Attribute);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
