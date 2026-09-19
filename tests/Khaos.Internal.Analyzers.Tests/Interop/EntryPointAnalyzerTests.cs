// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

public sealed class EntryPointAnalyzerTests
{
    [Test]
    public async Task MissingEntryPointReportsKHI003()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3))]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EntryPointAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI003");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,6): error KHI003: 'Quit' does not set EntryPoint; name the native symbol it binds");
    }

    [Test]
    public async Task EmptyEntryPointReportsKHI003()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "")]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EntryPointAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI003");
    }

    [Test]
    public async Task WhiteSpaceEntryPointReportsKHI003()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = " ")]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EntryPointAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI003");
    }

    [Test]
    public async Task ExplicitEntryPointReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
                internal static partial void Quit();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EntryPointAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task LibraryImportOutsideAnInteropAreaReportsNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Platform;

            internal static partial class Native
            {
                [LibraryImport("dwmapi")]
                internal static partial int Flush();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EntryPointAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
        await result.Reported.Should().BeEmpty();
    }

    [Test]
    public async Task GeneratedImplementationReportsKHI003Once()
    {
        const string source = """
            using System.CodeDom.Compiler;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3))]
                internal static partial void Quit();

                [GeneratedCode("Microsoft.Interop.LibraryImportGenerator", "11.0.0")]
                internal static partial void Quit() => throw new System.NotImplementedException();
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<EntryPointAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI003");
    }
}
