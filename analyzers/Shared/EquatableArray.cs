// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Shared;

/// <summary>
/// An immutable array that compares by value so it can safely live inside an incremental generator model.
/// </summary>
/// <remarks>
/// <see cref="ImmutableArray{T}"/> compares by reference. A model holding one is never equal to the model
/// produced by the previous run, which defeats the incremental cache and makes the generator re-emit on
/// every keystroke. A default (uninitialized) instance is treated as empty so both spellings compare and
/// hash the same way, which removes a subtle source of cache misses.
/// </remarks>
/// <typeparam name="T">The element type. It must compare by value all the way down.</typeparam>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T> where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array;

    internal EquatableArray(ImmutableArray<T> array) => _array = array;

    internal static readonly EquatableArray<T> Empty = new([]);

    internal int Length => _array.IsDefault ? 0 : _array.Length;

    internal bool IsEmpty => Length == 0;

    internal T this[int index] => AsImmutableArray()[index];

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

    public static implicit operator EquatableArray<T>(ImmutableArray<T> array) => new(array);

    internal ImmutableArray<T> AsImmutableArray() => _array.IsDefault ? [] : _array;

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

    public ImmutableArray<T>.Enumerator GetEnumerator() => AsImmutableArray().GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)AsImmutableArray()).GetEnumerator();
}
