// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

public sealed class InteropVisibilityAnalyzerTests
{
    [Test]
    public async Task PublicTypeInInteropAreaReportsKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            public readonly struct SDL_Window;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
        await result.Reported.Should().BeEqualTo("Source0.cs(3,24): error KHI005: 'SDL_Window' is visible outside the assembly; every type in an interop area is internal");
    }

    [Test]
    public async Task InternalTypeInInteropAreaReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal readonly struct SDL_Window;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeNestedInInternalTypeReportsNothing()
    {
        // A public type inside an internal one never leaves the assembly.
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal static class Handles
            {
                public readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeOutsideAnInteropAreaReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
