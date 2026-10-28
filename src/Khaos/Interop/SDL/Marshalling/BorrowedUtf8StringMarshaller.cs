// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Marshalling;

[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(BorrowedUtf8StringMarshaller))]
internal static class BorrowedUtf8StringMarshaller
{
    public static string? ConvertToManaged(byte* unmanaged) => Utf8StringMarshaller.ConvertToManaged(unmanaged);
}
