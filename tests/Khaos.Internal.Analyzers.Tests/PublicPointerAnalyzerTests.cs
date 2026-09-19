// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Internal.Analyzers.Tests;

public sealed class PublicPointerAnalyzerTests
{
    [Test]
    public async Task PublicVoidPointerPropertyReportsKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed unsafe class Window
            {
                public void* Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<PublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,18): error KHI008: 'Window.Handle' exposes a raw pointer; the pointer stays behind the wrapper");
    }

    [Test]
    public async Task PublicMethodWithPointerParameterReportsKHI008()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public static unsafe class Native
            {
                public static void Attach(void* handle)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<PublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI008");
        await result.Reported.Should().BeEqualTo("Source0.cs(5,24): error KHI008: 'Native.Attach' exposes a raw pointer; the pointer stays behind the wrapper");
    }

    [Test]
    public async Task PublicNintPropertyReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public sealed class Window
            {
                public nint Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<PublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task InternalVoidPointerPropertyReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal sealed unsafe class Window
            {
                internal void* Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<PublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task PublicPointerInsideAnInternalTypeReportsNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal sealed unsafe class Window
            {
                public void* Handle { get; }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<PublicPointerAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }
}
