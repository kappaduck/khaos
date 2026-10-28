// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KappaDuck.Khaos.Tests.Interop.SDL.Marshalling;

internal sealed partial class BorrowedSpanMarshallerTests
{
    [Test]
    public async Task AllocateContainerForManagedElementsShouldReadTheNativeElementsInPlace()
    {
        byte[] native = [1, 0, 1];
        ReadOnlySpan<byte> bytes;

        fixed (byte* ptr = native)
            bytes = BorrowedSpanMarshaller<byte, byte>.AllocateContainerForManagedElements(ptr, native.Length);

        await (bytes.Length == native.Length).Should().BeTrue();
        await Unsafe.AreSame(ref unsafe (MemoryMarshal.GetReference(bytes)), ref native[0]).Should().BeTrue();
    }

    [Test]
    public async Task AllocateContainerForManagedElementsShouldBeEmptyWhenThePointerIsNull()
    {
        ReadOnlySpan<byte> bytes = BorrowedSpanMarshaller<byte, byte>.AllocateContainerForManagedElements(null, 3);

        await bytes.IsEmpty.Should().BeTrue();
    }

    [Test]
    public async Task AllocateContainerForManagedElementsShouldRejectAnElementOfAnotherSize()
    {
        await Assert.That(static () => BorrowedSpanMarshaller<int, byte>.AllocateContainerForManagedElements(null, 0))
                    .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task AllocateContainerForManagedElementsShouldReadTheArrayThatSDLKeepsInPlace()
    {
        ReadOnlySpan<byte> first = GetKeyboardState(out int count);
        ReadOnlySpan<byte> second = GetKeyboardState(out _);

        bool overlaps = first.Overlaps(second, out int offset);

        await (first.Length == count).Should().BeTrue();
        await overlaps.Should().BeTrue();
        await offset.Should().BeZero();
    }

    [LibraryImport("SDL3", EntryPoint = "SDL_GetKeyboardState")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(BorrowedSpanMarshaller<,>), CountElementName = nameof(count))]
    private static safe partial ReadOnlySpan<byte> GetKeyboardState(out int count);
}
