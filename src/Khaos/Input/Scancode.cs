// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Input;

/// <summary>
/// The position of a key on the keyboard, named after the key at that place on a US keyboard. <see cref="W"/> is the
/// same key on a QWERTY and an AZERTY keyboard, so the controls of a game work on every layout.
/// </summary>
public enum Scancode
{
    /// <summary>No key, or a key Khaos does not know.</summary>
    Unknown = 0,

    /// <summary>The key at the place of A on a US keyboard.</summary>
    A = 4,

    /// <summary>The key at the place of B on a US keyboard.</summary>
    B = 5,

    /// <summary>The key at the place of C on a US keyboard.</summary>
    C = 6,

    /// <summary>The key at the place of D on a US keyboard.</summary>
    D = 7,

    /// <summary>The key at the place of E on a US keyboard.</summary>
    E = 8,

    /// <summary>The key at the place of F on a US keyboard.</summary>
    F = 9,

    /// <summary>The key at the place of G on a US keyboard.</summary>
    G = 10,

    /// <summary>The key at the place of H on a US keyboard.</summary>
    H = 11,

    /// <summary>The key at the place of I on a US keyboard.</summary>
    I = 12,

    /// <summary>The key at the place of J on a US keyboard.</summary>
    J = 13,

    /// <summary>The key at the place of K on a US keyboard.</summary>
    K = 14,

    /// <summary>The key at the place of L on a US keyboard.</summary>
    L = 15,

    /// <summary>The key at the place of M on a US keyboard.</summary>
    M = 16,

    /// <summary>The key at the place of N on a US keyboard.</summary>
    N = 17,

    /// <summary>The key at the place of O on a US keyboard.</summary>
    O = 18,

    /// <summary>The key at the place of P on a US keyboard.</summary>
    P = 19,

    /// <summary>The key at the place of Q on a US keyboard.</summary>
    Q = 20,

    /// <summary>The key at the place of R on a US keyboard.</summary>
    R = 21,

    /// <summary>The key at the place of S on a US keyboard.</summary>
    S = 22,

    /// <summary>The key at the place of T on a US keyboard.</summary>
    T = 23,

    /// <summary>The key at the place of U on a US keyboard.</summary>
    U = 24,

    /// <summary>The key at the place of V on a US keyboard.</summary>
    V = 25,

    /// <summary>The key at the place of W on a US keyboard.</summary>
    W = 26,

    /// <summary>The key at the place of X on a US keyboard.</summary>
    X = 27,

    /// <summary>The key at the place of Y on a US keyboard.</summary>
    Y = 28,

    /// <summary>The key at the place of Z on a US keyboard.</summary>
    Z = 29,

    /// <summary>The 1 key of the top row.</summary>
    Digit1 = 30,

    /// <summary>The 2 key of the top row.</summary>
    Digit2 = 31,

    /// <summary>The 3 key of the top row.</summary>
    Digit3 = 32,

    /// <summary>The 4 key of the top row.</summary>
    Digit4 = 33,

    /// <summary>The 5 key of the top row.</summary>
    Digit5 = 34,

    /// <summary>The 6 key of the top row.</summary>
    Digit6 = 35,

    /// <summary>The 7 key of the top row.</summary>
    Digit7 = 36,

    /// <summary>The 8 key of the top row.</summary>
    Digit8 = 37,

    /// <summary>The 9 key of the top row.</summary>
    Digit9 = 38,

    /// <summary>The 0 key of the top row.</summary>
    Digit0 = 39,

    /// <summary>The Enter key of the main keyboard.</summary>
    Enter = 40,

    /// <summary>The Esc key.</summary>
    Escape = 41,

    /// <summary>The Backspace key.</summary>
    Backspace = 42,

    /// <summary>The Tab key.</summary>
    Tab = 43,

    /// <summary>The space bar.</summary>
    Space = 44,

    /// <summary>The key at the place of - on a US keyboard.</summary>
    Minus = 45,

    /// <summary>The key at the place of = on a US keyboard.</summary>
    Equal = 46,

    /// <summary>The key at the place of [ on a US keyboard.</summary>
    LeftBracket = 47,

    /// <summary>The key at the place of ] on a US keyboard.</summary>
    RightBracket = 48,

    /// <summary>The key at the place of \ on a US keyboard.</summary>
    Backslash = 49,

    /// <summary>The key left of Enter on an ISO keyboard.</summary>
    NonUSHash = 50,

    /// <summary>The key at the place of ; on a US keyboard.</summary>
    Semicolon = 51,

    /// <summary>The key at the place of ' on a US keyboard.</summary>
    Apostrophe = 52,

    /// <summary>The key at the place of ` on a US keyboard, left of 1.</summary>
    Backquote = 53,

    /// <summary>The key at the place of , on a US keyboard.</summary>
    Comma = 54,

    /// <summary>The key at the place of . on a US keyboard.</summary>
    Period = 55,

    /// <summary>The key at the place of / on a US keyboard.</summary>
    Slash = 56,

    /// <summary>The Caps Lock key.</summary>
    CapsLock = 57,

    /// <summary>The F1 key.</summary>
    F1 = 58,

    /// <summary>The F2 key.</summary>
    F2 = 59,

    /// <summary>The F3 key.</summary>
    F3 = 60,

    /// <summary>The F4 key.</summary>
    F4 = 61,

    /// <summary>The F5 key.</summary>
    F5 = 62,

    /// <summary>The F6 key.</summary>
    F6 = 63,

    /// <summary>The F7 key.</summary>
    F7 = 64,

    /// <summary>The F8 key.</summary>
    F8 = 65,

    /// <summary>The F9 key.</summary>
    F9 = 66,

    /// <summary>The F10 key.</summary>
    F10 = 67,

    /// <summary>The F11 key.</summary>
    F11 = 68,

    /// <summary>The F12 key.</summary>
    F12 = 69,

    /// <summary>The Print Screen key.</summary>
    PrintScreen = 70,

    /// <summary>The Scroll Lock key.</summary>
    ScrollLock = 71,

    /// <summary>The Pause key.</summary>
    Pause = 72,

    /// <summary>The Insert key.</summary>
    Insert = 73,

    /// <summary>The Home key.</summary>
    Home = 74,

    /// <summary>The Page Up key.</summary>
    PageUp = 75,

    /// <summary>The Delete key.</summary>
    Delete = 76,

    /// <summary>The End key.</summary>
    End = 77,

    /// <summary>The Page Down key.</summary>
    PageDown = 78,

    /// <summary>The right arrow key.</summary>
    Right = 79,

    /// <summary>The left arrow key.</summary>
    Left = 80,

    /// <summary>The down arrow key.</summary>
    Down = 81,

    /// <summary>The up arrow key.</summary>
    Up = 82,

    /// <summary>The Num Lock key.</summary>
    NumLock = 83,

    /// <summary>The / key of the numeric keypad.</summary>
    KeypadDivide = 84,

    /// <summary>The * key of the numeric keypad.</summary>
    KeypadMultiply = 85,

    /// <summary>The - key of the numeric keypad.</summary>
    KeypadMinus = 86,

    /// <summary>The + key of the numeric keypad.</summary>
    KeypadPlus = 87,

    /// <summary>The Enter key of the numeric keypad.</summary>
    KeypadEnter = 88,

    /// <summary>The 1 key of the numeric keypad.</summary>
    Keypad1 = 89,

    /// <summary>The 2 key of the numeric keypad.</summary>
    Keypad2 = 90,

    /// <summary>The 3 key of the numeric keypad.</summary>
    Keypad3 = 91,

    /// <summary>The 4 key of the numeric keypad.</summary>
    Keypad4 = 92,

    /// <summary>The 5 key of the numeric keypad.</summary>
    Keypad5 = 93,

    /// <summary>The 6 key of the numeric keypad.</summary>
    Keypad6 = 94,

    /// <summary>The 7 key of the numeric keypad.</summary>
    Keypad7 = 95,

    /// <summary>The 8 key of the numeric keypad.</summary>
    Keypad8 = 96,

    /// <summary>The 9 key of the numeric keypad.</summary>
    Keypad9 = 97,

    /// <summary>The 0 key of the numeric keypad.</summary>
    Keypad0 = 98,

    /// <summary>The . key of the numeric keypad.</summary>
    KeypadPeriod = 99,

    /// <summary>The key between left Shift and Z on an ISO keyboard.</summary>
    NonUSBackslash = 100,

    /// <summary>The menu key, next to the right Ctrl.</summary>
    Application = 101,

    /// <summary>The left Ctrl key.</summary>
    LeftControl = 224,

    /// <summary>The left Shift key.</summary>
    LeftShift = 225,

    /// <summary>The left Alt key.</summary>
    LeftAlt = 226,

    /// <summary>The left Windows key.</summary>
    LeftMeta = 227,

    /// <summary>The right Ctrl key.</summary>
    RightControl = 228,

    /// <summary>The right Shift key.</summary>
    RightShift = 229,

    /// <summary>The right Alt key, AltGr on many keyboards.</summary>
    RightAlt = 230,

    /// <summary>The right Windows key.</summary>
    RightMeta = 231
}
