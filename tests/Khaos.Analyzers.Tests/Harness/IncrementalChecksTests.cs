// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Tests.Harness.Fixtures;

namespace KappaDuck.Khaos.Analyzers.Tests.Harness;

internal sealed class IncrementalChecksTests
{
    private const string Player = "public sealed partial class Player;";
    private const string Enemy = "public sealed partial class Enemy;";

    [Test]
    public async Task RerunShouldPassWhenTheModelComparesByValue()
    {
        string failures = IncrementalChecks.Rerun<TypeNameGenerator>([Player, Enemy], TypeNameGenerator.Names);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task RerunShouldFailWhenTheModelHoldsASymbol()
    {
        string failures = IncrementalChecks.Rerun<SymbolGenerator>([Player], SymbolGenerator.Symbols);

        await failures.Should().Contain("'Symbols' holds a ISymbol");
    }

    [Test]
    public async Task RerunShouldFailWhenTheTrackingNameDoesNotExist()
    {
        string failures = IncrementalChecks.Rerun<TypeNameGenerator>([Player], "Missing");

        await failures.Should().Contain("No tracked step named 'Missing'");
    }

    [Test]
    public async Task EditShouldPassWhenAnEditThatKeepsTheModelReusesTheStep()
    {
        string failures = IncrementalChecks.Edit<TypeNameGenerator>([Player, Enemy], [Player, "// A comment.\n" + Enemy], [TypeNameGenerator.Names], []);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task EditShouldPassWhenAnEditThatChangesTheModelRecomputesTheStep()
    {
        string failures = IncrementalChecks.Edit<TypeNameGenerator>([Player, Enemy], [Player, "public sealed partial class Boss;"], [], [TypeNameGenerator.Names]);

        await failures.Should().BeEmpty();
    }

    [Test]
    public async Task EditShouldFailWhenAStepExpectedToBeReusedIsRecomputed()
    {
        string failures = IncrementalChecks.Edit<TypeNameGenerator>([Player, Enemy], [Player, "public sealed partial class Boss;"], [TypeNameGenerator.Names], []);

        await failures.Should().Contain("'Names' was recomputed");
    }
}
