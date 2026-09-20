// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Interop.SDL.Marshalling;
using System.Text;

namespace KappaDuck.Khaos.Tests.Interop.SDL.Marshalling;

public sealed class OwnedUtf8StringMarshallerTests
{
    [Test]
    [Arguments("")]
    [Arguments("café")]
    [Arguments("Khaos!")]
    [Arguments("a string with spaces")]
    [Arguments("a duck 🦆 and a duckling 🐤")]
    public async Task ConvertToManagedCopiesTheString(string value)
    {
        string? marshalled = Convert(value);
        await marshalled.Should().BeEqualTo(value);
    }

    [Test]
    public async Task ConvertToManagedShouldReturnNullWhenPointerIsNull()
    {
        string? marshalled = unsafe (BorrowedUtf8StringMarshaller.ConvertToManaged(null));
        await marshalled.Should().BeNull();
    }

    private static string? Convert(string value)
    {
        ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> text = stackalloc byte[bytes.Length + 1];

        bytes.CopyTo(text);
        text[^1] = 0;

        fixed (byte* ptr = text)
            return unsafe (OwnedUtf8StringMarshaller.ConvertToManaged(ptr));
    }
}
