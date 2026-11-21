// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using KappaDuck.Khaos.Tools.Tests.Helpers;

namespace KappaDuck.Khaos.Tools.Tests.Native;

internal sealed class CompatibilityTableTests : IDisposable
{
    private const string ReadmePath = "readme.md";

    private const string Readme = """
        # Khaos

        ## SDL compatibility

        |  Khaos   | Native  |   SDL3   |
        | :------: | :-----: | :------: |
        | `source` | `1.2.1` | `3.4.20` |

        ## After the table

        This line stays.
        """;

    private const string Released = """
        ## SDL compatibility

        |  Khaos   | Native  |   SDL3   | SDL_image |
        | :------: | :-----: | :------: | :-------: |
        | `source` | `1.3.0` | `3.4.22` |  `3.4.8`  |
        | `0.2.0`  | `1.2.1` | `3.4.20` |  `3.4.8`  |
        | `0.1.0`  | `1.2.0` | `3.4.20` |  `3.4.6`  |
        """;

    private static readonly Version _khaos = new(0, 1, 0);
    private static readonly Version _native = new(1, 2, 1);
    private static readonly Dictionary<string, Version> _bundled = new() { ["SDL3"] = new Version(3, 4, 22) };

    private readonly Workspace _workspace = Workspace.Empty();
    private readonly string _path;

    public CompatibilityTableTests() => _path = _workspace.Write(ReadmePath, Readme);

    public void Dispose() => _workspace.Dispose();

    [Test]
    public async Task SaveAsyncShouldWriteTheReleaseRowUnderTheSourceRow()
    {
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        table.AddRelease(_khaos, _native);
        await table.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task AddReleaseShouldRefuseASecondRowForTheSameRelease()
    {
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);
        table.AddRelease(_khaos, _native);

        await Assert.That(() => table.AddRelease(_khaos, _native))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage($"The SDL compatibility table already has a row for Khaos {_khaos}.");
    }

    [Test]
    public async Task AddReleaseShouldRefuseANativeTheSourceRowDoesNotShip()
    {
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        await Assert.That(() => table.AddRelease(_khaos, new Version(1, 2, 2)))
                    .ThrowsExactly<InvalidOperationException>()
                    .WithMessage("Khaos references the native packages 1.2.2, but the `source` row of the SDL compatibility table ships 1.2.1. Update one of them first.");
    }

    [Test]
    public async Task AddReleaseShouldRefuseATableWithoutSourceRow()
    {
        _workspace.Write(ReadmePath, Readme.Replace("`source`", "`0.0.1`", StringComparison.Ordinal));
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        await Assert.That(() => table.AddRelease(_khaos, _native))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"The first row of the SDL compatibility table in '{_path}' must be the `source` row.");
    }

    [Test]
    public async Task ParseAsyncShouldRefuseAReadmeWithoutTheTable()
    {
        _workspace.Write(ReadmePath, "# Khaos\n");

        await Assert.That(async () => await CompatibilityTable.ParseAsync(_path))
                    .ThrowsExactly<FormatException>();
    }

    [Test]
    public async Task ParseAsyncShouldRefuseATableWithoutNativeColumn()
    {
        _workspace.Write(ReadmePath, Readme.Replace("Native ", "Engine ", StringComparison.Ordinal));

        await Assert.That(async () => await CompatibilityTable.ParseAsync(_path))
                    .ThrowsExactly<FormatException>();
    }

    [Test]
    public async Task SetSourceShouldReplaceTheSourceRow()
    {
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        table.SetSource(new Version(1, 2, 2), _bundled);
        await table.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task SetSourceShouldAddTheSourceRowAboveTheLastRelease()
    {
        _workspace.Write(ReadmePath, Readme.Replace("`source`", "`0.1.0`", StringComparison.Ordinal));

        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        table.SetSource(new Version(1, 2, 2), _bundled);
        await table.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task SetSourceShouldAddTheSourceRowToAnEmptyTable()
    {
        _workspace.Write(ReadmePath, "## SDL compatibility\n\n| Khaos | Native | SDL3 |\n| :-: | :-: | :-: |");

        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        table.SetSource(new Version(1, 2, 2), _bundled);
        await table.SaveAsync();

        await VerifyFile(_path);
    }

    [Test]
    public async Task SetSourceShouldRefuseAColumnWithoutVersion()
    {
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        await Assert.That(() => table.SetSource(_native, new Dictionary<string, Version>()))
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"Unknown column 'SDL3' in the SDL compatibility table of '{_path}'.");
    }

    [Test]
    public async Task ChangedSinceReleaseShouldReturnEveryLibraryBeforeTheFirstRelease()
    {
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        IReadOnlyList<KeyValuePair<string, Version>> libraries = table.ChangedSinceRelease();

        await libraries[0].Key.Should().BeEqualTo("SDL3");
        await libraries[0].Value.Should().BeEqualTo(new Version(3, 4, 20));
    }

    [Test]
    public async Task ChangedSinceReleaseShouldReturnTheLibrariesThatDifferFromTheLastRelease()
    {
        _workspace.Write(ReadmePath, Released);
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        IReadOnlyList<KeyValuePair<string, Version>> libraries = table.ChangedSinceRelease();

        await libraries[0].Key.Should().BeEqualTo("SDL3");
        await libraries[0].Value.Should().BeEqualTo(new Version(3, 4, 22));
    }

    [Test]
    public async Task ChangedSinceReleaseShouldReturnNothingWhenTheSourceRowMatchesTheLastRelease()
    {
        _workspace.Write(ReadmePath, Released.Replace("`3.4.22`", "`3.4.20`", StringComparison.Ordinal));
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        IReadOnlyList<KeyValuePair<string, Version>> libraries = table.ChangedSinceRelease();

        await libraries.Should().BeEmpty();
    }

    [Test]
    public async Task ChangedSinceReleaseShouldRefuseATableWithoutSourceRow()
    {
        _workspace.Write(ReadmePath, Readme.Replace("`source`", "`0.0.1`", StringComparison.Ordinal));
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        await Assert.That(table.ChangedSinceRelease)
                    .ThrowsExactly<FormatException>();
    }

    [Test]
    public async Task ChangedSinceReleaseShouldRefuseASourceCellThatIsNotAVersion()
    {
        _workspace.Write(ReadmePath, Readme.Replace("`3.4.20`", "`main`", StringComparison.Ordinal));
        CompatibilityTable table = await CompatibilityTable.ParseAsync(_path);

        await Assert.That(table.ChangedSinceRelease)
                    .ThrowsExactly<FormatException>()
                    .WithMessage($"'`main`' is not a version in the SDL compatibility table of '{_path}'.");
    }
}
