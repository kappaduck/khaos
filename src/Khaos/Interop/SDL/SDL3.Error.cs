// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Diagnostics;
using System.Numerics;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ThrowIfFailed([DoesNotReturnIf(false)] bool succeeded, [CallerArgumentExpression(nameof(succeeded))] string call = "")
    {
        if (!succeeded)
            Throw(call);
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string call = "") where T : ISignedNumber<T>
    {
        if (T.IsNegative(value))
            Throw(call);

        return value;
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T ThrowIfNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string call = "") where T : class
    {
        if (value is null)
            Throw(call);

        return value;
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T* ThrowIfNull<T>(T* handle, [CallerArgumentExpression(nameof(handle))] string call = "") where T : unmanaged
    {
        if (handle is null)
            Throw(call);

        return handle;
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T** ThrowIfNull<T>(T** handle, [CallerArgumentExpression(nameof(handle))] string call = "") where T : unmanaged
    {
        if (handle is null)
            Throw(call);

        return handle;
    }

    [DebuggerHidden]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string call = "") where T : INumber<T>
    {
        if (T.IsZero(value))
            Throw(call);

        return value;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Throw(string call)
    {
        string? error = GetError();
        ClearError();

        int arguments = call.IndexOf('(', StringComparison.Ordinal);
        string function = arguments <= 0 ? call : call[..arguments];

        string reason = string.IsNullOrEmpty(error) ? "SDL reported no error message" : error;

        throw new KhaosInteropException($"{function} failed: {reason}");
    }

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_ClearError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static safe partial void ClearError();

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    private static safe partial string? GetError();
}
