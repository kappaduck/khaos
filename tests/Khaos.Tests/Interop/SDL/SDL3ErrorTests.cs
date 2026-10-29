// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KappaDuck.Khaos.Tests.Interop.SDL;

internal sealed partial class SDL3ErrorTests
{
    [Test]
    public async Task ThrowIfFailedShouldNotThrowWhenTheCallSucceeded()
    {
        await Assert.That(static () => SDL3.ThrowIfFailed(true)).ThrowsNothing();
    }

    [Test]
    public async Task ThrowIfFailedShouldThrowAPlatformExceptionWhenTheCallFailed()
    {
        Action check = static () => SDL3.ThrowIfFailed(false);

        await check.Should().ThrowExactly<PlatformException>();
    }

    [Test]
    public async Task ThrowIfFailedShouldNameTheCallInTheMessage()
    {
        PlatformException exception = Assert.Throws<PlatformException>(static () => SDL3.ThrowIfFailed(Hide(succeeded: false)));

        await exception.Message.Should().StartWith("Hide(succeeded: false) failed: ");
    }

    [Test]
    public async Task ThrowIfNullShouldReturnThePointerWhenItIsNotNull()
    {
        bool same = ThrowIfNullReturnsTheSamePointer();

        await same.Should().BeTrue();
    }

    [Test]
    public async Task ThrowIfNullShouldThrowWhenThePointerIsNull()
    {
        Action check = static () => SDL3.ThrowIfNull<int>(null);

        await check.Should().ThrowExactly<PlatformException>();
    }

    [Test]
    public async Task ThrowIfZeroShouldReturnTheIdWhenItIsNotZero()
    {
        const uint id = 7;

        uint result = SDL3.ThrowIfZero(id);

        await result.Should().BeEqualTo(id);
    }

    [Test]
    public async Task ThrowIfZeroShouldThrowWhenTheIdIsZero()
    {
        Action check = static () => SDL3.ThrowIfZero(0u);

        await check.Should().ThrowExactly<PlatformException>();
    }

    [Test]
    public async Task ThrowIfZeroShouldPutTheErrorOfTheSystemInTheMessage()
    {
        const string call = "GetWindowIdOfNoWindow() failed: ";

        PlatformException exception = Assert.Throws<PlatformException>(static () => SDL3.ThrowIfZero(GetWindowIdOfNoWindow()));

        string error = exception.Message[call.Length..];
        await error.Should().NotBeEmpty();
    }

    [Test]
    public async Task ThrowIfNegativeShouldReturnTheCountWhenItIsZeroOrMore()
    {
        const int count = 0;

        int result = SDL3.ThrowIfNegative(count);

        await result.Should().BeEqualTo(count);
    }

    [Test]
    public async Task ThrowIfNegativeShouldThrowWhenTheCountIsNegative()
    {
        Action check = static () => SDL3.ThrowIfNegative(-1L);

        await check.Should().ThrowExactly<PlatformException>();
    }

    [Test]
    public async Task PlatformExceptionShouldBeAKhaosException()
    {
        Action check = static () => throw new PlatformException("The window could not be created.");

        await check.Should().Throw<KhaosException>();
    }

    private static bool Hide(bool succeeded) => succeeded;

    private static bool ThrowIfNullReturnsTheSamePointer()
    {
        int value = 42;

        int* pointer = &value;
        return SDL3.ThrowIfNull(pointer) == pointer;
    }

    private static uint GetWindowIdOfNoWindow() => unsafe (GetWindowId(null));

    [LibraryImport("SDL3", EntryPoint = "SDL_GetWindowID")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe partial uint GetWindowId(void* window);
}
