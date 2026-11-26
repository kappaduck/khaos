// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared.Unions;

namespace KappaDuck.Khaos.Internal.Analyzers.Events.Helpers;

internal sealed record EventCase(
    UnionCase Union,
    uint First,
    uint Last,
    string FirstLabel,
    string LastLabel,
    string? NativeField,
    bool TakesContext)
{
    internal bool IsRange => First != Last;

    internal bool Overlaps(EventCase other) => First <= other.Last && other.First <= Last;
}
