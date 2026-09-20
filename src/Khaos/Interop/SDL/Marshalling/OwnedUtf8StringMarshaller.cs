// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Interop.SDL.Marshalling;

[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(OwnedUtf8StringMarshaller))]
internal static class OwnedUtf8StringMarshaller
{
    internal static string? ConvertToManaged(byte* unmanaged) => Utf8StringMarshaller.ConvertToManaged(unmanaged);

    internal static void Free(byte* unmanaged) => SDL3.Free(unmanaged);
}
