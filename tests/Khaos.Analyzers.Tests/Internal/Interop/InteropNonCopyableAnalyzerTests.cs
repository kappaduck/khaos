// Copyright (c) KappaDuck.
// Licensed under the MIT license.

extern alias InternalAnalyzers;

using InternalAnalyzers::KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Analyzers.Tests.Internal.Interop;

internal sealed class InteropNonCopyableAnalyzerTests
{
    private const string Lease = """
        namespace KappaDuck.Khaos.Interop.SDL;

        [NonCopyable]
        internal struct Lease : System.IDisposable
        {
            internal int Count;

            internal void Release()
            {
            }

            internal void Dispose(bool disposing)
            {
            }

            public void Dispose()
            {
            }
        }
        """;

    private const string Buffer = """
        namespace KappaDuck.Khaos.Interop
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, Inherited = false)]
            internal sealed class NonCopyableAttribute : System.Attribute;
        }

        namespace KappaDuck.Khaos.Interop.SDL
        {
            [NonCopyable]
            internal readonly ref struct OwnedBuffer<T> : System.IDisposable where T : unmanaged
            {
                internal int Length => 0;

                public System.ReadOnlySpan<T>.Enumerator GetEnumerator() => default(System.ReadOnlySpan<T>).GetEnumerator();

                public void Dispose()
                {
                }
            }

            internal static class SDL3
            {
                internal static OwnedBuffer<uint> GetKeyboards() => new();

                internal static OwnedBuffer<uint> GetMice() => new();
            }
        }
        """;

    [Test]
    public async Task CreationReadAsATemporaryShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static int Count() => SDL3.GetKeyboards().Length;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(7,36): error KHI012: This 'OwnedBuffer<uint>' has no using to own it; declare it with using, or return it to the caller");
    }

    [Test]
    public async Task CopyIntoAnotherUsingShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static int Count()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();
                    using OwnedBuffer<uint> copy = keyboards;
                    return copy.Length;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(10,40): error KHI012: 'keyboards' is copied; read it through its members, or pass it by in");
    }

    [Test]
    public async Task DisposingByHandShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static void Release()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();
                    keyboards.Dispose();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Reported.Should().BeEqualTo("Source0.cs(10,9): error KHI012: 'keyboards' is disposed by hand; its using already disposes it");
    }

    [Test]
    [Arguments("internal static void Touch() { {|KHI012:SDL3.GetKeyboards()|}; }")]
    [Arguments("internal static void Drop() { _ = {|KHI012:SDL3.GetKeyboards()|}; }")]
    [Arguments("internal static int Count() { OwnedBuffer<uint> keyboards = {|KHI012:SDL3.GetKeyboards()|}; return keyboards.Length; }")]
    [Arguments("internal static void Replace() { OwnedBuffer<uint> keyboards; keyboards = {|KHI012:SDL3.GetKeyboards()|}; }")]
    [Arguments("internal static int Count() => Measure({|KHI012:SDL3.GetKeyboards()|}); private static int Measure(in OwnedBuffer<uint> keyboards) => keyboards.Length;")]
    [Arguments("internal static int Count() => {|KHI012:new OwnedBuffer<uint>()|}.Length;")]
    [Arguments("internal static int Count(bool all) => (all ? {|KHI012:SDL3.GetKeyboards()|} : {|KHI012:SDL3.GetMice()|}).Length;")]
    public async Task CreationWithoutAUsingShouldReportKHI012(string member)
    {
        Markup source = Markup.Parse(Keyboards(member), Buffer);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    [Arguments("internal static int Count() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); OwnedBuffer<uint> copy = {|KHI012:keyboards|}; return copy.Length; }")]
    [Arguments("internal static void Drop() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); _ = {|KHI012:keyboards|}; }")]
    [Arguments("internal static int Count() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); return Measure({|KHI012:keyboards|}); } private static int Measure(OwnedBuffer<uint> keyboards) => keyboards.Length;")]
    [Arguments("internal static OwnedBuffer<uint> All() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); return {|KHI012:keyboards|}; }")]
    [Arguments("internal static int Count(bool all) { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); using OwnedBuffer<uint> copy = all ? {|KHI012:keyboards|} : default; return copy.Length; }")]
    [Arguments("internal static bool Any() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); return {|KHI012:keyboards|} is var copy && copy.Length > 0; }")]
    [Arguments("internal static int Count() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); return {|KHI012:keyboards|} is { Length: > 0 } copy ? copy.Length : 0; }")]
    [Arguments("internal static int Count() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); return {|KHI012:keyboards|} switch { { Length: 0 } => 0, var copy => copy.Length }; }")]
    [Arguments("internal static int Count() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); switch ({|KHI012:keyboards|}) { case { Length: 0 }: return 0; case var copy: return copy.Length; } }")]
    [Arguments("internal static int Count() { using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards(); return {|KHI012:keyboards|} is { Length: > 0 } and var copy ? copy.Length : 0; }")]
    public async Task CopyShouldReportKHI012(string member)
    {
        Markup source = Markup.Parse(Keyboards(member), Buffer);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task ParameterCopiedOrDisposedShouldReportKHI012()
    {
        Markup source = Markup.Parse(Keyboards("""
                internal static int Measure(in OwnedBuffer<uint> keyboards)
                {
                    OwnedBuffer<uint> copy = {|KHI012:keyboards|};
                    {|KHI012:keyboards.Dispose()|};
                    return copy.Length;
                }
            """), Buffer);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
        await result.Reported.Should().Contain("'keyboards' is copied");
        await result.Reported.Should().Contain("'keyboards' is disposed by hand");
    }

    [Test]
    public async Task ValueHandedOverThroughOutShouldReportKHI012()
    {
        Markup source = Markup.Parse("""
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Native
            {
                internal static bool TryGetKeyboards(out OwnedBuffer<uint> keyboards)
                {
                    keyboards = {|KHI012:new()|};
                    return true;
                }

                internal static int Count()
                {
                    if (!TryGetKeyboards(out OwnedBuffer<uint> {|KHI012:keyboards|}))
                        return 0;

                    return keyboards.Length;
                }
            }
            """, Buffer);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task CreationInAFileMarkedGeneratedShouldReportKHI012()
    {
        Markup source = Markup.Parse("""
            // <auto-generated/>
            namespace KappaDuck.Khaos.Input
            {
                using KappaDuck.Khaos.Interop.SDL;

                internal static class Keyboards
                {
                    internal static int Count() => {|KHI012:SDL3.GetKeyboards()|}.Length;
                }
            }
            """, Buffer);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEqualTo(source.Expected);
    }

    [Test]
    public async Task ReadsThroughMembersShouldReportNothing()
    {
        string source = Keyboards("""
                internal static uint Sum()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();

                    uint sum = (uint)keyboards.Length;
                    foreach (uint id in keyboards)
                        sum += id;

                    return sum + (uint)Measure(in keyboards) + (uint)nameof(keyboards).Length;
                }

                internal static int Kind()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();

                    if (keyboards is { Length: 0 })
                        return 0;

                    switch (keyboards)
                    {
                        case { Length: 1 }:
                            return 1;
                    }

                    return keyboards switch
                    {
                        { Length: 2 } => 2,
                        _ => 3
                    };
                }

                private static int Measure(in OwnedBuffer<uint> keyboards) => Read(in keyboards);

                private static int Read(in OwnedBuffer<uint> keyboards) => keyboards.Length;
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task RefParameterPassedOnByRefShouldReportNothing()
    {
        string source = Keyboards("""
                internal static int Measure(ref OwnedBuffer<uint> keyboards) => Read(ref keyboards);

                private static int Read(ref OwnedBuffer<uint> keyboards) => keyboards.Length;
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task UsingStatementShouldReportNothing()
    {
        string source = Keyboards("""
                internal static int Count()
                {
                    using (OwnedBuffer<uint> keyboards = SDL3.GetKeyboards())
                        return keyboards.Length;
                }

                internal static void Touch()
                {
                    using (SDL3.GetKeyboards())
                    {
                    }
                }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task ReturningACreationShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Wrappers
            {
                internal static OwnedBuffer<uint> Keyboards() => SDL3.GetKeyboards();

                internal static OwnedBuffer<uint> Nothing()
                {
                    return unsafe (new OwnedBuffer<uint>());
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task CreationThroughAConditionalOrASwitchShouldReportNothing()
    {
        string source = Keyboards("""
                internal static OwnedBuffer<uint> Either(bool keyboards) => keyboards ? SDL3.GetKeyboards() : SDL3.GetMice();

                internal static OwnedBuffer<uint> Of(int kind)
                {
                    return kind switch
                    {
                        0 => SDL3.GetKeyboards(),
                        _ => (SDL3.GetMice())
                    };
                }

                internal static int Count(bool keyboards)
                {
                    using OwnedBuffer<uint> devices = keyboards ? SDL3.GetKeyboards() : SDL3.GetMice();
                    return devices.Length;
                }
            """);

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task TypeWithoutNonCopyableShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            internal readonly ref struct Lease : System.IDisposable
            {
                public void Dispose()
                {
                }
            }

            internal static class Leases
            {
                internal static void Copy()
                {
                    Lease lease = new();
                    Lease copy = lease;
                    copy.Dispose();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task NonCopyableFromAnotherNamespaceShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            [System.AttributeUsage(System.AttributeTargets.Struct)]
            internal sealed class NonCopyableAttribute : System.Attribute;

            [NonCopyable]
            internal readonly ref struct Lease : System.IDisposable
            {
                public void Dispose()
                {
                }
            }

            internal static class Leases
            {
                internal static void Copy()
                {
                    Lease lease = new();
                    Lease copy = lease;
                    copy.Dispose();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task CompilationWithoutNonCopyableShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            internal readonly ref struct Lease : System.IDisposable
            {
                public void Dispose()
                {
                }
            }

            internal static class Leases
            {
                internal static void Copy()
                {
                    Lease lease = new();
                    Lease copy = lease;
                    copy.Dispose();
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task FieldReadMethodGroupAndOtherDisposeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Interop.SDL;

            internal static class Leases
            {
                internal static int Count()
                {
                    using Lease lease = new();
                    return lease.Count;
                }

                internal static System.Action Releaser()
                {
                    using Lease lease = new();
                    return lease.Release;
                }

                internal static void Flush()
                {
                    using Lease lease = new();
                    lease.Dispose(false);
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer, Lease);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    [Test]
    public async Task LocalOfAnArrayTypeShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            internal static class Counts
            {
                internal static int Total()
                {
                    int[] counts = [1, 2];
                    int[] copy = counts;
                    return copy.Length;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Located.Should().BeEmpty();
    }

    private static string Keyboards(string members) => $$"""
        namespace KappaDuck.Khaos.Input;

        using KappaDuck.Khaos.Interop.SDL;

        internal static class Keyboards
        {
        {{members}}
        }
        """;
}
