// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Input;
using KappaDuck.Khaos.Tests.Helpers;

namespace KappaDuck.Khaos.Tests.Input;

internal sealed class ScancodeTests
{
    [Test]
    public async Task UnknownShouldBeZero()
    {
        const int value = (int)Scancode.Unknown;

        await value.Should().BeEqualTo(0);
    }

    [Test]
    [Arguments(Scancode.A, "A")]
    [Arguments(Scancode.Z, "Z")]
    [Arguments(Scancode.Digit1, "1")]
    [Arguments(Scancode.Digit0, "0")]
    [Arguments(Scancode.Enter, "Return")]
    [Arguments(Scancode.Escape, "Escape")]
    [Arguments(Scancode.Backspace, "Backspace")]
    [Arguments(Scancode.Tab, "Tab")]
    [Arguments(Scancode.Space, "Space")]
    [Arguments(Scancode.Slash, "/")]
    [Arguments(Scancode.CapsLock, "CapsLock")]
    [Arguments(Scancode.F1, "F1")]
    [Arguments(Scancode.F12, "F12")]
    [Arguments(Scancode.Delete, "Delete")]
    [Arguments(Scancode.Right, "Right")]
    [Arguments(Scancode.Up, "Up")]
    [Arguments(Scancode.NumLock, "Numlock")]
    [Arguments(Scancode.KeypadEnter, "Keypad Enter")]
    [Arguments(Scancode.KeypadPeriod, "Keypad .")]
    [Arguments(Scancode.Application, "Application")]
    [Arguments(Scancode.LeftControl, "Left Ctrl")]
    [Arguments(Scancode.LeftShift, "Left Shift")]
    [Arguments(Scancode.LeftAlt, "Left Alt")]
    [Arguments(Scancode.LeftMeta, "Left GUI")]
    [Arguments(Scancode.RightMeta, "Right GUI")]
    public async Task ValueShouldBeTheScancodeThatSDLNamesTheSameKey(Scancode code, string name)
    {
        string native = SDLKeyboard.ScancodeName(code);

        await native.Should().BeEqualTo(name);
    }
}
