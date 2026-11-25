// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;

namespace KappaDuck.Khaos.Analyzers.Tests.Shared;

internal sealed class CodeSourceBuilderTests : IDisposable
{
    private const int RentedLength = 1024;

    private readonly CodeSourceBuilder _builder = new();

    public void Dispose() => _builder.Dispose();

    public static IEnumerable<Func<(string, Action<CodeSourceBuilder, string>)>> LineWriters()
    {
        yield return static () => ("AppendLine", static (builder, text) => builder.AppendLine(text));
        yield return static () => ("AppendLineStringHole", static (builder, text) => builder.AppendLine($"{text}"));
        yield return static () => ("AppendLineSpanHole", static (builder, text) => builder.AppendLine($"{text.AsSpan()}"));
        yield return static () => ("Append", static (builder, text) => builder.Append(text).AppendLine());
        yield return static () => ("AppendStringHole", static (builder, text) => builder.Append($"{text}").AppendLine());
    }

    [Test]
    public async Task BuilderShouldStartWithTheGeneratedHeader()
    {
        _builder.AppendLine("namespace Game;");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AppendLineShouldWriteTheLine()
    {
        _builder.AppendLine("public sealed class Khaos;");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AppendLineShouldNeverWriteACarriageReturn()
    {
        _builder.AppendLine("int a;\r\nint b;");

        string source = _builder.ToString();

        await source.Should().NotContain("\r");
    }

    [Test]
    public async Task AppendLineWithoutTextShouldWriteAnEmptyLine()
    {
        _builder.AppendLine("int a;").AppendLine().AppendLine("").AppendLine("int b;");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AppendShouldContinueTheLine()
    {
        _builder.Append("private ").Append("int ").AppendLine("Value;");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AStringHoleShouldWriteTheString()
    {
        const string name = "Shape";
        const string? missing = null;

        _builder.AppendLine($"partial struct {name};")
                .AppendLine($"partial struct {missing}Circle;");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task ACharHoleShouldWriteTheChar()
    {
        const char quote = '"';

        _builder.AppendLine($"string text = {quote}{quote};");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task ASpanHoleShouldWriteTheSpan()
    {
        const string name = "Shape.Circle";

        _builder.AppendLine($"partial struct {name.AsSpan(6)};");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AnIntHoleShouldWriteItsDigits()
    {
        _builder.AppendLine($"const int Zero = {0};")
                .AppendLine($"const int One = {1};")
                .AppendLine($"const int MinusOne = {-1};")
                .AppendLine($"const int Max = {int.MaxValue};")
                .AppendLine($"const int Min = {int.MinValue};");

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task InterpolatedAppendLineShouldNotAllocate()
    {
        const string name = "Shape";
        const int tag = 3;

        _builder.AppendLine($"private const byte {name} = {tag};");

        long before = GC.GetAllocatedBytesForCurrentThread();

        _builder.AppendLine($"private const byte {name} = {tag};");

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        await allocated.Should().BeEqualTo(0L);
    }

    [Test]
    public async Task AppendBlockShouldIndentItsLines()
    {
        using (_builder.AppendBlock("partial struct Shape"))
        {
            _builder.AppendLine("private readonly byte _tag;");
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task NestedBlocksShouldIndentFourSpacesPerLevel()
    {
        using (_builder.AppendBlock("namespace Game"))
        {
            using (_builder.AppendBlock("partial struct Shape"))
            {
                _builder.AppendLine("private readonly byte _tag;");
            }
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AnEmptyLineInABlockShouldHaveNoTrailingSpaces()
    {
        using (_builder.AppendBlock("partial struct Shape"))
        {
            _builder.AppendLine("int a;").AppendLine().AppendLine("int b;");
        }

        await _builder.ToString().Should().NotContain(" \n");
    }

    [Test]
    public async Task AppendBlockShouldCloseWithTheGivenClosing()
    {
        using (_builder.AppendBlock("public override string ToString() => _tag switch", "};"))
        {
            _builder.AppendLine("_ => \"\"");
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AppendBlockShouldTakeAnInterpolatedLine()
    {
        const string name = "Shape";

        using (_builder.AppendBlock($"partial struct {name}"))
        {
            _builder.AppendLine("private readonly byte _tag;");
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task AppendBlockAfterAppendShouldContinueTheLine()
    {
        _builder.Append("public ");

        using (_builder.AppendBlock("partial struct Shape"))
        {
            _builder.AppendLine("private readonly byte _tag;");
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task LineFeedsInALineShouldIndentEachLine()
    {
        const string summary = "/// <summary>\n/// Holds one shape.\n/// </summary>";

        using (_builder.AppendBlock("partial struct Shape"))
        {
            _builder.AppendLine(summary)
                    .AppendLine(summary);
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    [MethodDataSource(nameof(LineWriters))]
    public async Task ALineLongerThanTwiceTheBufferShouldBeKeptWhole(string name, Action<CodeSourceBuilder, string> writeLine)
    {
        string text = new('x', RentedLength * 5);

        writeLine(_builder, text);

        string source = _builder.ToString();

        await Verify(source, "cs").UseParameters(name);
    }

    [Test]
    [Arguments(-1)]
    [Arguments(0)]
    [Arguments(1)]
    public async Task ALineEndingAroundTheFirstBufferShouldBeKeptWhole(int offset)
    {
        const string Header = "// <auto-generated/>\n#nullable enable\n\n";
        string text = new('x', RentedLength - Header.Length - 1 + offset);

        _builder.AppendLine(text);

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task ManyIndentedLinesShouldBeKeptWhole()
    {
        using (_builder.AppendBlock("namespace Game"))
        {
            using (_builder.AppendBlock("partial struct Shape"))
            {
                for (int i = 0; i < 100; i++)
                    _builder.AppendLine($"private const int Value{i} = {i};");
            }
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }

    [Test]
    public async Task ToStringShouldReturnEmptyWhenIsDisposed()
    {
        CodeSourceBuilder builder = new();

        builder.Dispose();
        string source = builder.ToString();

        await source.Should().BeEqualTo(string.Empty);
    }

    [Test]
    [MethodDataSource(nameof(LineWriters))]
    public async Task WritingAfterDisposeShouldThrow(string _, Action<CodeSourceBuilder, string> writeLine)
    {
        CodeSourceBuilder builder = new();
        builder.Dispose();

        Action write = () => writeLine(builder, "x");

        await write.Should().Throw<ObjectDisposedException>();
    }

    [Test]
    public async Task DisposingTwiceShouldNotThrow()
    {
        CodeSourceBuilder builder = new();

        builder.Dispose();
        builder.Dispose();

        await builder.ToString().Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task BuilderShouldWriteAGeneratedType()
    {
        const string name = "Shape";

        _builder.AppendLine("namespace Game;").AppendLine();

        using (_builder.AppendBlock($"partial struct {name}"))
        {
            _builder.AppendLine("private readonly byte _tag;").AppendLine();

            using (_builder.AppendBlock("public override string ToString() => _tag switch", "};"))
            {
                for (int i = 0; i < 3; i++)
                    _builder.AppendLine($"{i} => \"Case{i}\",");

                _builder.AppendLine("_ => \"\"");
            }
        }

        string source = _builder.ToString();

        await Verify(source, "cs");
    }
}
