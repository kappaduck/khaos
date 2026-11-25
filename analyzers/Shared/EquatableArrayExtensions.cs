// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Shared;

internal static class EquatableArrayExtensions
{
    extension<T>(ImmutableArray<T> array) where T : IEquatable<T>
    {
        internal EquatableArray<T> ToEquatableArray() => new(array);
    }

    extension<T>(IEnumerable<T> source) where T : IEquatable<T>
    {
        internal EquatableArray<T> ToEquatableArray() => new([.. source]);
    }
}
