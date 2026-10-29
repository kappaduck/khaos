// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Diagnostics;
using System.Numerics;

namespace KappaDuck.Khaos.Interop.SDL;

internal static partial class SDL3
{
    internal static void ThrowIfFailed([DoesNotReturnIf(false)] bool succeeded, [CallerArgumentExpression(nameof(succeeded))] string call = "")
    {
        if (!succeeded)
            Throw(call);
    }

    internal static T* ThrowIfNull<T>(T* result, [CallerArgumentExpression(nameof(result))] string call = "") where T : unmanaged
    {
        if (result is null)
            Throw(call);

        return result;
    }

    internal static T ThrowIfZero<T>(T result, [CallerArgumentExpression(nameof(result))] string call = "") where T : INumberBase<T>
    {
        if (T.IsZero(result))
            Throw(call);

        return result;
    }

    internal static T ThrowIfNegative<T>(T result, [CallerArgumentExpression(nameof(result))] string call = "") where T : INumberBase<T>
    {
        if (T.IsNegative(result))
            Throw(call);

        return result;
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void Throw(string call) => throw new PlatformException($"{call} failed: {GetError()}");

    [LibraryImport(nameof(SDL3), EntryPoint = "SDL_GetError")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    private static safe partial string? GetError();
}
