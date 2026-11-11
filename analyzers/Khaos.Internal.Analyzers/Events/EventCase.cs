// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

internal sealed record EventCase(
    string Name,
    string TypeName,
    uint First,
    uint Last,
    string FirstLabel,
    string LastLabel,
    string? NativeField,
    bool TakesContext,
    bool Managed)
{
    internal bool IsRange => First != Last;

    internal bool Overlaps(EventCase other) => First <= other.Last && other.First <= Last;
}
