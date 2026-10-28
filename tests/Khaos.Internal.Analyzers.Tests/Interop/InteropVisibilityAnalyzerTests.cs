// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

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
        await result.Ids.Should().BeEqualTo("KHI005");
        await result.Reported.Should().BeEqualTo("Source0.cs(3,24): error KHI005: 'SDL_Window' is visible outside the assembly; every type in Interop is internal");
    }

    [Test]
    public async Task PublicClassInWin32AreaShouldReportKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Win32;

            public static partial class User32;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
        await result.Reported.Should().BeEqualTo("Source0.cs(3,29): error KHI005: 'User32' is visible outside the assembly; every type in Interop is internal");
    }

    [Test]
    public async Task PublicEnumInSDLAreaShouldReportKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL.Primitives;

            public enum SDL_Scancode;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
    }

    [Test]
    public Task PublicTypeNestedInPublicTypeShouldReportBoth()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public static class SDL3
            {
                public readonly struct SDL_Window;
            }
            """;

        return AnalyzerRunner<InteropVisibilityAnalyzer>.VerifyAsync([source]);
    }

    [Test]
    public async Task PartialPublicTypeShouldReportKHI005Once()
    {
        const string first = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public static partial class SDL3;
            """;

        const string second = """
            namespace KappaDuck.Khaos.Interop.SDL;

            static partial class SDL3;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(first, second);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task PrivateProtectedTypeNestedInPublicTypeShouldReportOnlyItsContainer()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public class Container
            {
                private protected readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
        await result.Reported.Should().Contain("'Container' is visible outside the assembly");
    }

    [Test]
    public async Task ProtectedTypeNestedInPublicTypeShouldReportBoth()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public class Container
            {
                protected readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005, KHI005");
        await result.Reported.Should().Contain("'SDL_Window' is visible outside the assembly");
    }

    [Test]
    public async Task ProtectedInternalTypeNestedInPublicTypeShouldReportBoth()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public class Container
            {
                protected internal readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005, KHI005");
        await result.Reported.Should().Contain("'SDL_Window' is visible outside the assembly");
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
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task ExtensionBlockInPublicTypeShouldReportOnlyItsContainer()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public static class SDL3
            {
                extension(int value)
                {
                    public bool IsZero => value == 0;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
        await result.Reported.Should().Contain("'SDL3' is visible outside the assembly");
    }

    [Test]
    public async Task PublicDelegateInSDLAreaShouldReportKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            public delegate void SDL_Callback();
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
    }

    [Test]
    public async Task PublicTypeInGeneratedCodeShouldReportKHI005()
    {
        const string source = """
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL
            {
                public readonly struct SDL_Window;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task PublicTypeAtTheInteropRootShouldReportKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop;

            public sealed class UntypedBecauseAttribute(string reason) : System.Attribute
            {
                public string Reason { get; } = reason;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
        await result.Reported.Should().Contain("'UntypedBecauseAttribute' is visible outside the assembly");
    }

    [Test]
    public async Task PublicTypeInAnUnlistedInteropAreaShouldReportKHI005()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.Browser;

            public static partial class KhaosScript;
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropVisibilityAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI005");
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
    }
}
