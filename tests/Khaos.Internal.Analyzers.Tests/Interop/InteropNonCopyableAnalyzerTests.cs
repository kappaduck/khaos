// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Internal.Analyzers.Interop;

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Interop;

internal sealed class InteropNonCopyableAnalyzerTests
{
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
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().BeEqualTo("Source0.cs(7,36): error KHI012: This 'OwnedBuffer<uint>' has no using to own it; declare it with using, or return it to the caller");
    }

    [Test]
    public async Task CreationKeptInALocalWithoutUsingShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static int Count()
                {
                    OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();
                    return keyboards.Length;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().Contain("Source0.cs(9,39)");
        await result.Reported.Should().Contain("has no using to own it");
    }

    [Test]
    public async Task CreationPassedAsAnArgumentShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static int Count() => Measure(SDL3.GetKeyboards());

                private static int Measure(in OwnedBuffer<uint> keyboards) => keyboards.Length;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().Contain("has no using to own it");
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
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().BeEqualTo("Source0.cs(10,40): error KHI012: 'keyboards' is copied; read it through its members, or pass it by in");
    }

    [Test]
    public async Task PassingByValueShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static int Count()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();
                    return Measure(keyboards);
                }

                private static int Measure(OwnedBuffer<uint> keyboards) => keyboards.Length;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().Contain("Source0.cs(10,24): error KHI012: 'keyboards' is copied");
    }

    [Test]
    public async Task ReturningALocalOwnedByAUsingShouldReportKHI012()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static OwnedBuffer<uint> All()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();
                    return keyboards;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().Contain("'keyboards' is copied");
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
        await result.Ids.Should().BeEqualTo("KHI012");
        await result.Reported.Should().BeEqualTo("Source0.cs(10,9): error KHI012: 'keyboards' is disposed by hand; its using already disposes it");
    }

    [Test]
    public async Task UsingDeclarationShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static uint Sum()
                {
                    using OwnedBuffer<uint> keyboards = SDL3.GetKeyboards();

                    uint sum = (uint)keyboards.Length;
                    foreach (uint id in keyboards)
                        sum += id;

                    return sum + (uint)Measure(in keyboards) + (uint)nameof(keyboards).Length;
                }

                private static int Measure(in OwnedBuffer<uint> keyboards) => keyboards.Length;
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
    }

    [Test]
    public async Task UsingStatementShouldReportNothing()
    {
        const string source = """
            namespace KappaDuck.Khaos.Input;

            using KappaDuck.Khaos.Interop.SDL;

            internal static class Keyboards
            {
                internal static int Count()
                {
                    using (OwnedBuffer<uint> keyboards = SDL3.GetKeyboards())
                        return keyboards.Length;
                }
            }
            """;

        AnalyzerResult result = await AnalyzerRunner<InteropNonCopyableAnalyzer>.RunAsync(source, Buffer);

        await result.CompilationErrors.Should().BeEmpty();
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
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
        await result.Ids.Should().BeEmpty();
    }

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
            }
        }
        """;
}
