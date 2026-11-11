// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

internal static class EventUnion
{
    internal static EquatableArray<EventCase> Resolve(IEnumerable<EventCase> cases) => Resolve(cases, conflicts: null);

    internal static EquatableArray<EventCase> Resolve(IEnumerable<EventCase> cases, List<(EventCase Dropped, EventCase Kept)>? conflicts)
    {
        List<EventCase> kept = [];

        foreach (EventCase @case in cases.OrderBy(static @case => @case.First).ThenBy(static @case => @case.TypeName, StringComparer.Ordinal))
        {
            EventCase? clash = kept.Find(@case.Overlaps);

            if (clash is null)
                kept.Add(@case);
            else
                conflicts?.Add((@case, clash));
        }

        return kept.ToEquatableArray();
    }
}
