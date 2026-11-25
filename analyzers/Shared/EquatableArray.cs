// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Shared;

internal readonly struct EquatableArray<T>(ImmutableArray<T> array) : IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array = array;

    internal static readonly EquatableArray<T> Empty = new([]);

    internal bool IsEmpty => Length == 0;

    internal int Length => _array.IsDefault ? 0 : _array.Length;

    public bool Equals(EquatableArray<T> other)
    {
        if (IsEmpty)
            return other.IsEmpty;

        if (Length != other.Length)
            return false;

        for (int i = 0; i < _array.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_array[i], other._array[i]))
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (IsEmpty)
            return 0;

        int hash = 17;

        foreach (T item in _array)
            hash = unchecked((hash * 31) + (item?.GetHashCode() ?? 0));

        return hash;
    }

    internal ReadOnlySpan<T> AsSpan() => _array.IsDefault ? [] : _array.AsSpan();
}
