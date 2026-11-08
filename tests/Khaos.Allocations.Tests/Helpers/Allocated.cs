// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

namespace KappaDuck.Khaos.Allocations.Tests.Helpers;

internal static class Allocated
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long By(Action action)
    {
        action();

        long before = GC.GetAllocatedBytesForCurrentThread();
        action();

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long By<TState>(TState state, Action<TState> action)
    {
        action(state);

        long before = GC.GetAllocatedBytesForCurrentThread();
        action(state);

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static long By<TState, TResult>(TState state, Func<TState, TResult> func)
    {
        _ = func(state);

        long before = GC.GetAllocatedBytesForCurrentThread();
        _ = func(state);

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
