// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

internal sealed class InteropCallbackSignatureAnalyzerTests
{
    [Test]
    public async Task UnmanagedCallersOnlyReturningBoolShouldReportKHI010()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class EventFilter
            {
                [UnmanagedCallersOnly]
                internal static bool Filter(void* userdata) => true;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(8,26): error KHI010: 'Filter' has bool in a native callback signature; it is not blittable and Native AOT stops the process on it, use byte or int");
    }

    [Test]
    public async Task UnmanagedCallersOnlyWithCharOutsideInteropShouldReportKHI010()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Input;

            internal static class TextInput
            {
                [UnmanagedCallersOnly]
                internal static void OnCharacter(char character)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(8,26): error KHI010: 'OnCharacter' has char in a native callback signature; it is not blittable and Native AOT stops the process on it, use byte or ushort");
    }

    [Test]
    public async Task ConstructorWithAnUnmanagedFunctionPointerShouldReportKHI010ByTheTypeName()
    {
        const string source = """
            namespace KappaDuck.Khaos.Windowing;

            internal sealed class Hooks
            {
                internal Hooks(delegate* unmanaged<void*, bool> filter)
                {
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(5,14): error KHI010: 'Hooks' has bool in a native callback signature; it is not blittable and Native AOT stops the process on it, use byte or int");
    }

    [Test]
    [Arguments("[UnmanagedCallersOnly] internal static void {|KHI010:OnKey|}(bool down) { }")]
    [Arguments("[UnmanagedCallersOnly] internal static char {|KHI010:Next|}() => 'a';")]
    [Arguments("[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])] internal static bool {|KHI010:Filter|}(void* userdata) => true;")]
    [Arguments("[UnmanagedCallersOnly] internal static void {|KHI010:Register|}(delegate* unmanaged<bool, void> filter) { }")]
    public async Task UnmanagedCallersOnlyMethodShouldReportKHI010(string member)
    {
        Markup source = Markup.Parse($$"""
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Callbacks
            {
                {{member}}
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("internal delegate* unmanaged<void*, bool> {|KHI010:filter|};")]
    [Arguments("internal delegate* unmanaged[Cdecl]<char, void> {|KHI010:Typed|} { get; set; }")]
    [Arguments("internal delegate* unmanaged[Cdecl, SuppressGCTransition]<bool, void> {|KHI010:fast|};")]
    [Arguments("internal delegate* unmanaged<bool, void>* {|KHI010:slot|};")]
    [Arguments("internal delegate* unmanaged<bool, void>[] {|KHI010:filters|} = [];")]
    [Arguments("internal delegate* unmanaged<delegate* unmanaged<bool, void>, void> {|KHI010:register|};")]
    [Arguments("internal delegate*<delegate* unmanaged<bool, void>, void> {|KHI010:install|};")]
    [Arguments("internal static delegate* unmanaged<char> {|KHI010:Get|}() => null;")]
    [Arguments("internal static void {|KHI010:Set|}(delegate* unmanaged<void*, bool> filter) { }")]
    [Arguments("internal int {|KHI010:this|}[delegate* unmanaged<bool, void> filter] => 0;")]
    public async Task UnmanagedFunctionPointerShouldReportKHI010(string member)
    {
        Markup source = Markup.Parse($$"""
            namespace KappaDuck.Khaos.Windowing;

            internal sealed class Hooks
            {
                {{member}}
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task UnmanagedFunctionPointerParameterOfALibraryImportShouldReportKHI010()
    {
        Markup source = Markup.Parse(InteropSources.SDL("""
                [LibraryImport("SDL3", EntryPoint = "SDL_SetEventFilter")]
                internal static partial void {|KHI010:SetEventFilter|}(delegate* unmanaged<void*, void*, bool> filter, void* userdata);
            """));

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("[UnmanagedCallersOnly] static bool {|KHI010:Filter|}(void* userdata) => true;")]
    [Arguments("static void {|KHI010:Register|}(delegate* unmanaged<bool, void> filter) { }")]
    public async Task LocalFunctionShouldReportKHI010(string function)
    {
        Markup source = Markup.Parse($$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class EventFilter
            {
                internal static void Install()
                {
                    {{function}}
                }
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task DelegateWithAnUnmanagedFunctionPointerShouldReportKHI010()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL;

            internal delegate void {|KHI010:Hook|}(delegate* unmanaged<void*, bool> filter);
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task CallbackInAFileMarkedGeneratedShouldReportKHI010()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class EventFilter
            {
                [UnmanagedCallersOnly]
                internal static bool {|KHI010:Filter|}(void* userdata) => true;
            }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task FunctionPointerParameterWithItsGeneratedHalfShouldReportKHI010Once()
    {
        const string generated = """
            // <auto-generated/>
            namespace KappaDuck.Khaos.Interop.SDL
            {
                internal static partial class SDL3
                {
                    [global::System.Runtime.CompilerServices.SkipLocalsInitAttribute]
                    internal static partial byte AddEventWatch(delegate* unmanaged<void*, void*, bool> filter, void* userdata)
                    {
                        byte __retVal;
                        unsafe
                        {
                            __retVal = __PInvoke(filter, userdata);
                        }

                        return __retVal;

                        [global::System.Runtime.InteropServices.DllImportAttribute("SDL3", EntryPoint = "SDL_AddEventWatch", ExactSpelling = true)]
                        static extern unsafe byte __PInvoke(delegate* unmanaged<void*, void*, bool> __filter_native, void* __userdata_native);
                    }
                }
            }
            """;

        Markup source = Markup.Parse(InteropSources.SDL("""
                [LibraryImport("SDL3", EntryPoint = "SDL_AddEventWatch")]
                internal static partial byte {|KHI010:AddEventWatch|}(delegate* unmanaged<void*, void*, bool> filter, void* userdata);
            """), generated);

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task BlittableCallbackShouldReportNothing()
    {
        const string source = """
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class EventFilter
            {
                [UnmanagedCallersOnly]
                internal static byte Filter(void* userdata) => 1;

                internal static delegate* unmanaged<void*, byte> Pointer => &Filter;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    [Arguments("internal delegate*<bool, void> managed;")]
    [Arguments("internal delegate*<char> Managed { get; set; }")]
    public async Task ManagedFunctionPointerWithBoolOrCharShouldReportNothing(string member)
    {
        string source = $$"""
            namespace KappaDuck.Khaos.Windowing;

            internal struct Hooks
            {
                {{member}}
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    [Arguments("bool*")]
    [Arguments("char*")]
    public async Task PointerInACallbackShouldReportNothing(string type)
    {
        string source = $$"""
            using System.Runtime.InteropServices;

            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Callbacks
            {
                [UnmanagedCallersOnly]
                internal static void Fill({{type}} values)
                {
                }

                internal static delegate* unmanaged<{{type}}, void> Pointer => &Fill;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task BoolWithoutUnmanagedCallersOnlyShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal sealed class Errors
            {
                internal bool Failed { get; }

                internal int this[char code] => code;

                internal static bool IsFailure(byte result)
                {
                    return Check(result);

                    static bool Check(byte value) => value == 0;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task ManagedDelegateWithBoolOrCharShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal delegate bool Predicate(char character);
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropCallbackSignatureAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }
}
