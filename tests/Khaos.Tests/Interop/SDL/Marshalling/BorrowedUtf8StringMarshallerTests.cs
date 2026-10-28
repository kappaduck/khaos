// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KappaDuck.Khaos.Tests.Interop.SDL.Marshalling;

internal sealed partial class BorrowedUtf8StringMarshallerTests
{
    [Test]
    public async Task ConvertToManagedShouldDecodeUtf8()
    {
        ReadOnlySpan<byte> native = "Khaos é\0"u8;
        string? text;

        fixed (byte* ptr = native)
            text = BorrowedUtf8StringMarshaller.ConvertToManaged(ptr);

        await text.Should().BeEqualTo("Khaos é");
    }

    [Test]
    public async Task ConvertToManagedShouldReturnNullWhenThePointerIsNull()
    {
        string? text = BorrowedUtf8StringMarshaller.ConvertToManaged(null);

        await text.Should().BeNull();
    }

    [Test]
    public async Task ConvertToManagedShouldDecodeAStringThatSDLKeeps()
    {
        string expected = OperatingSystem.IsWindows() ? "Windows" : "Linux";

        string? platform = GetPlatform();

        await platform.Should().BeEqualTo(expected);
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_GetPlatform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedUtf8StringMarshaller))]
    private static safe partial string? GetPlatform();
}
