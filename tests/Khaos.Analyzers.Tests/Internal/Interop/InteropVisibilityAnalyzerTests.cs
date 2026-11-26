// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

internal sealed class InteropVisibilityAnalyzerTests
{
    [Test]
    public async Task PublicStructInSDLAreaShouldReportKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            public readonly struct SDL_Window;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(3,24): error KHI005: 'SDL_Window' is visible outside the assembly; every type in Interop is internal");
    }

    [Test]
    [Arguments("public static partial class {|KHI005:SDL3|};")]
    [Arguments("public enum {|KHI005:SDL_Scancode|};")]
    [Arguments("public delegate void {|KHI005:SDL_Callback|}();")]
    [Arguments("public interface {|KHI005:ISDL_Stream|};")]
    [Arguments("public readonly record struct {|KHI005:SDL_Point|}(int X, int Y);")]
    public async Task PublicTypeOfAnyKindInSDLAreaShouldReportKHI005(string declaration)
    {
        Markup source = Markup.Parse($$"""
            namespace KappaDuck.Khaos.Interop.SDL;

            {{declaration}}
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicClassInWin32AreaShouldReportKHI005()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.Win32;

            public static partial class {|KHI005:User32|};
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicTypeNestedInPublicTypeShouldReportBoth()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL;

            public static class {|KHI005:SDL3|}
            {
                public readonly struct {|KHI005:SDL_Window|};
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("protected")]
    [Arguments("protected internal")]
    public async Task TypeNestedInPublicTypeAndSeenByDerivedTypesShouldReportBoth(string modifier)
    {
        Markup source = Markup.Parse($$"""
            namespace KappaDuck.Khaos.Interop.SDL;

            public class {|KHI005:Container|}
            {
                {{modifier}} readonly struct {|KHI005:SDL_Window|};
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PrivateProtectedTypeNestedInPublicTypeShouldReportOnlyItsContainer()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL;

            public class {|KHI005:Container|}
            {
                private protected readonly struct SDL_Window;
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task ExtensionBlockInPublicTypeShouldReportOnlyItsContainer()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL;

            public static class {|KHI005:SDL3|}
            {
                extension(int value)
                {
                    public bool IsZero => value == 0;
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PartialPublicTypeShouldReportKHI005Once()
    {
        Markup source = Markup.Parse(
            """
            namespace KappaDuck.Khaos.Interop.SDL;

            public static partial class {|KHI005:SDL3|};
            """,
            """
            namespace KappaDuck.Khaos.Interop.SDL;

            static partial class SDL3;
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicTypeInGeneratedCodeShouldReportKHI005()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL
            {
                public readonly struct {|KHI005:SDL_Window|};
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicTypeAtTheInteropRootShouldReportKHI005()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop;

            public sealed class {|KHI005:UntypedBecauseAttribute|}(string reason) : System.Attribute
            {
                public string Reason { get; } = reason;
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task PublicTypeInAnUnlistedInteropAreaShouldReportKHI005()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.Browser;

            public static partial class {|KHI005:KhaosScript|};
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task InternalStructInSDLAreaShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal readonly struct SDL_Window;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeNestedInInternalTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            internal static class Handles
            {
                public readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task ProtectedTypeNestedInInternalTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal class Container
            {
                protected readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task FileTypeInSDLAreaShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            file static class Helpers;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeOutsideInteropShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            public readonly struct WindowHandle;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeInAnInteropNamespaceOutsideTheRootShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing.Interop;

            public readonly struct WindowHandle;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeInAnotherProductsInteropShouldReportNothing()
    {
        const string source = """
            namespace Fake.Khaos.Interop.SDL;

            public readonly struct SDL_Window;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task InteropClassCompletedByLibraryImportShouldReportNothing()
    {
        const string declaration = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static partial class SDL3
            {
                [LibraryImport(nameof(SDL3), EntryPoint = "SDL_Quit")]
                internal static unsafe partial void Quit();
            }
            """;

        const string generated = """
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL
            {
                internal static partial class SDL3
                {
                    [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_Quit", ExactSpelling = true)]
                    internal static unsafe extern partial void Quit();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(declaration, generated);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }
}
