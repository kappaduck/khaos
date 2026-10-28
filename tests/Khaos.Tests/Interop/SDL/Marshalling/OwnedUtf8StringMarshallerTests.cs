// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KappaDuck.Khaos.Tests.Interop.SDL.Marshalling;

internal sealed partial class OwnedUtf8StringMarshallerTests
{
    [Test]
    public async Task ConvertToManagedShouldDecodeAStringThatSDLAllocated()
    {
        byte* bytes = SDLMemory.Copy("Khaos é\0"u8);

        string? text = OwnedUtf8StringMarshaller.ConvertToManaged(bytes);
        OwnedUtf8StringMarshaller.Free(bytes);

        await text.Should().BeEqualTo("Khaos é");
    }

    [Test]
    public async Task ConvertToManagedShouldReturnNullWhenThePointerIsNull()
    {
        string? text = OwnedUtf8StringMarshaller.ConvertToManaged(null);
        OwnedUtf8StringMarshaller.Free(null);

        await text.Should().BeNull();
    }

    [Test]
    public async Task ConvertToManagedShouldDecodeAStringThatSDLDuplicated()
    {
        string? text = Duplicate("Khaos é");

        await text.Should().BeEqualTo("Khaos é");
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_strdup", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(OwnedUtf8StringMarshaller))]
    private static safe partial string? Duplicate(string text);
}
