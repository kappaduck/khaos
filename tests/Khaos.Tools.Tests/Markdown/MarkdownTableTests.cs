// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Markdown;

namespace KappaDuck.Khaos.Tools.Tests.Markdown;

internal sealed class MarkdownTableTests
{
    private const string _header = "| Name | Version |";
    private const string _separator = "| :--- | :----: |";
    private const string _row = "| Khaos | 0.1.0 |";

    [Test]
    public async Task ParseShouldThrowWhenLinesHaveOnlyTheHeader()
    {
        await Assert.That(() => MarkdownTable.Parse([_header]))
                    .ThrowsExactly<FormatException>()
                    .WithMessage("A markdown table needs a header and a separator row.");
    }

    [Test]
    public async Task ParseShouldNotThrowWhenLinesHaveHeaderAndSeparator()
    {
        await Assert.That(() => MarkdownTable.Parse([_header, _separator]))
                    .ThrowsNothing();
    }

    [Test]
    public async Task ParseShouldParseTheHeader()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator]);

        await table.Headers[0].Should().BeEqualTo("Name");
        await table.Headers[1].Should().BeEqualTo("Version");
    }

    [Test]
    public async Task ParseShouldNotThrowsWhenAlignmentsAndHeaderHaveSameLength()
    {
        const string header = "| Name | Version | Link |";
        const string separator = "| :--- | :---: | ---: |";

        await Assert.That(() => MarkdownTable.Parse([header, separator]))
                    .ThrowsNothing();
    }

    [Test]
    public async Task ParseShouldThrowsWhenAlignmentIsEmpty()
    {
        const string alignment = "| :--- | :---: | : |";

        await Assert.That(() => MarkdownTable.Parse([_header, alignment]))
                    .ThrowsExactly<FormatException>()
                    .WithMessage("':' is not a valid separator cell.");
    }

    [Test]
    [Arguments("x--x")]
    [Arguments("x")]
    [Arguments(":xx:")]
    public async Task ParseShouldThrowsWhenAlignmentHasInvalidCharacter(string alignment)
    {
        string alignments = $"| :--- | :---: | {alignment} |";

        await Assert.That(() => MarkdownTable.Parse([_header, alignment]))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'{alignment}' is not a valid separator cell.");
    }

    [Test]
    public async Task ParseShouldThrowsWhenAlignmentsAndHeaderHaveNotSameLength()
    {
        const string alignment = "| :--- | :---: | ----: |";

        await Assert.That(() => MarkdownTable.Parse([_header, alignment]))
                    .ThrowsExactly<FormatException>()
                    .WithMessage("The separator row has 3 cells, but the header has 2.");
    }

    [Test]
    public async Task ParseShouldParseTheBody()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        ReadOnlySpan<string> row = table[0];
        await row[0].Should().BeEqualTo("Khaos");
        await row[1].Should().BeEqualTo("0.1.0");
    }

    [Test]
    public async Task ParseShouldCountTheRow()
    {
        const string anotherRow = "| Woah | 0.2.0 |";

        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row, anotherRow]);

        await table.RowCount.Should().BeEqualTo(2);
    }

    [Test]
    public async Task ParseShouldFillForEmptyCell()
    {
        const string line = "| Khaos | |";

        MarkdownTable table = MarkdownTable.Parse([_header, _separator, line]);

        ReadOnlySpan<string> row = table[0];
        await row[0].Should().BeEqualTo("Khaos");
        await row[1].Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task ParseShouldFillForRowThanIsLessThanHeaderColumn()
    {
        const string line = "| Khaos |";

        MarkdownTable table = MarkdownTable.Parse([_header, _separator, line]);

        ReadOnlySpan<string> row = table[0];

        await row.Length.Should().BeEqualTo(2);
        await row[0].Should().BeEqualTo("Khaos");
        await row[1].Should().BeEqualTo(string.Empty);
    }

    [Test]
    public async Task ParseShouldTruncateRowWhenIsGreaterThanHeaderColumn()
    {
        const string line = "| Khaos | 0.1.0 | Link |";

        MarkdownTable table = MarkdownTable.Parse([_header, _separator, line]);

        ReadOnlySpan<string> row = table[0];

        await row.Length.Should().BeEqualTo(2);
        await row[0].Should().BeEqualTo("Khaos");
        await row[1].Should().BeEqualTo("0.1.0");
    }

    [Test]
    public async Task OperatorJaggedArrayShouldAccessTheCellFromTheBody()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await table[0, 0].Should().BeEqualTo("Khaos");
        await table[0, 1].Should().BeEqualTo("0.1.0");
    }

    [Test]
    public async Task OperatorIndexByRowAndColumnShouldAccessTheCellFromTheBody()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await table[0, 0].Should().BeEqualTo("Khaos");
        await table[0, 1].Should().BeEqualTo("0.1.0");
    }

    [Test]
    public async Task OperatorIndexByRowAndColumnShouldModifyTheCellFromTheBody()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        table[0, 1] = "0.2.0";

        await table[0, 1].Should().BeEqualTo("0.2.0");
    }

    [Test]
    [Arguments("0.2.0|")]
    [Arguments("|0.2.0")]
    [Arguments("|0.2.0|")]
    [Arguments("0.2.0\n")]
    [Arguments("0.2.0\r")]
    [Arguments("0.|2.0")]
    [Arguments("\r0.|2.0")]
    public async Task OperatorIndexByRowAndColumnShouldThrowsWhenReceivingInvalidCharacter(string version)
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table[0, 1] = version)
                    .ThrowsExactly<ArgumentException>()
                    .WithMessage($"A cell cannot contain '|' or a line break: '{version}'. (Parameter 'cell')");
    }

    [Test]
    public async Task ColumnIndexOfShouldGiveTheIndexFromHeader()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        int index = table.ColumnIndexOf("Version");

        await index.Should().BeEqualTo(1);
    }

    [Test]
    public async Task ColumnIndexOfShouldGiveMinusOneWhenSearchFromBodyValue()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        int index = table.ColumnIndexOf("0.1.0");

        await index.Should().BeEqualTo(-1);
    }

    [Test]
    public async Task RowIndexOfShouldGiveTheIndexForTheFirstRowWithTheValue()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row, _row]);

        int column = table.ColumnIndexOf("Version");
        int index = table.RowIndexOf(column, "0.1.0");

        await index.Should().BeZero();
    }

    [Test]
    public async Task RowIndexOfShouldGiveMinusOneWhenSearchFromBodyValue()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        int index = table.RowIndexOf(0, "0.4.0");

        await index.Should().BeEqualTo(-1);
    }

    [Test]
    public async Task RowIndexOfShouldThrowsWhenColumnIsNegative()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table.RowIndexOf(-1, "0.1.0"))
                    .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    [Arguments(2)]
    [Arguments(3)]
    public async Task RowIndexOfShouldThrowsWhenColumnIsGreaterThanOrEqualToHeader(int column)
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table.RowIndexOf(column, "0.1.0"))
                    .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task InsertShouldInsertNewRow()
    {
        ReadOnlySpan<string> cells = ["Link", "0.4.0"];
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        table.Insert(0, cells);

        ReadOnlySpan<string> row = table[0];

        await row[0].Should().BeEqualTo("Link");
        await row[1].Should().BeEqualTo("0.4.0");
    }

    [Test]
    public async Task InsertShouldThrowWhenCellsHaveNotSameLengthAsHeader()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table.Insert(0, ["Link", "0.4.0", "Another Link"]))
                    .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    [Arguments("0.2.0|")]
    [Arguments("|0.2.0")]
    [Arguments("|0.2.0|")]
    [Arguments("0.2.0\n")]
    [Arguments("0.2.0\r")]
    [Arguments("0.|2.0")]
    [Arguments("\r0.|2.0")]
    public async Task InsertShouldThrowWhenCellHasInvalidCharacter(string version)
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table.Insert(0, ["Link", version]))
                    .ThrowsExactly<ArgumentException>()
                    .WithMessage($"A cell cannot contain '|' or a line break: '{version}'. (Parameter 'cell')");
    }

    [Test]
    public async Task ReplaceShouldReplaceTheRow()
    {
        ReadOnlySpan<string> cells = ["Link", "0.4.0"];
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        table.Replace(0, cells);

        ReadOnlySpan<string> row = table[0];

        await row[0].Should().BeEqualTo("Link");
        await row[1].Should().BeEqualTo("0.4.0");
    }

    [Test]
    public async Task ReplaceShouldThrowWhenCellsHaveNotSameLengthAsHeader()
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table.Replace(0, ["Link", "0.4.0", "Another Link"]))
                    .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    [Arguments("0.2.0|")]
    [Arguments("|0.2.0")]
    [Arguments("|0.2.0|")]
    [Arguments("0.2.0\n")]
    [Arguments("0.2.0\r")]
    [Arguments("0.|2.0")]
    [Arguments("\r0.|2.0")]
    public async Task ReplaceShouldThrowWhenCellHasInvalidCharacter(string version)
    {
        MarkdownTable table = MarkdownTable.Parse([_header, _separator, _row]);

        await Assert.That(() => table.Replace(0, ["Link", version]))
                    .ThrowsExactly<ArgumentException>()
                    .WithMessage($"A cell cannot contain '|' or a line break: '{version}'. (Parameter 'cell')");
    }

    [Test]
    public async Task RenderShouldAlignEveryColumnAndKeepItsAlignment()
    {
        const string header = "| Name | Version | Size |";
        const string separator = "| :-- | :-: | --: |";

        MarkdownTable table = MarkdownTable.Parse([header, separator]);
        table.Insert(0, "SDL_image", "3.4.8", "1");

        string[] lines = table.Render();

        await Verify(string.Join('\n', lines), "md");
    }
}
