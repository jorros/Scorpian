using System;
using static Scorpian.SDL.SDL;

namespace Scorpian.InputManagement;

public enum KeyboardKey
{
    Unknown = 0,

    A = 4,
    B = 5,
    C = 6,
    D = 7,
    E = 8,
    F = 9,
    G = 10,
    H = 11,
    I = 12,
    J = 13,
    K = 14,
    L = 15,
    M = 16,
    N = 17,
    O = 18,
    P = 19,
    Q = 20,
    R = 21,
    S = 22,
    T = 23,
    U = 24,
    V = 25,
    W = 26,
    X = 27,
    Y = 28,
    Z = 29,

    D1 = 30,
    D2 = 31,
    D3 = 32,
    D4 = 33,
    D5 = 34,
    D6 = 35,
    D7 = 36,
    D8 = 37,
    D9 = 38,
    D0 = 39,

    Enter = 40,
    Escape = 41,
    Backspace = 42,
    Tab = 43,
    Space = 44,

    OemMinus = 45,
    Equals = 46,
    OemOpenBrackets = 47,
    OemCloseBrackets = 48,
    OemBackslash = 49,
    NonUsHash = 50,
    OemSemicolon = 51,
    Apostrophe = 52,
    Grave = 53,
    OemComma = 54,
    OemPeriod = 55,
    OemSlash = 56,

    Capslock = 57,

    F1 = 58,
    F2 = 59,
    F3 = 60,
    F4 = 61,
    F5 = 62,
    F6 = 63,
    F7 = 64,
    F8 = 65,
    F9 = 66,
    F10 = 67,
    F11 = 68,
    F12 = 69,

    PrintScreen = 70,
    Scroll = 71,
    Pause = 72,
    Insert = 73,
    Home = 74,
    PageUp = 75,
    Delete = 76,
    End = 77,
    PageDown = 78,
    Right = 79,
    Left = 80,
    Down = 81,
    Up = 82,
    
    LeftControl = 224,
    LeftShift = 225,
    LeftAlt = 226,
    LeftGui = 227,
    RightControl = 228,
    RightShift = 229,
    RightAlt = 230,
    RightGui = 231,
}

public static class KeyboardSdlExtension
{
    internal static KeyboardKey ToKey(this SDL_Scancode code)
    {
        if (Enum.IsDefined(typeof(KeyboardKey), (int) code))
        {
            return (KeyboardKey) code;
        }

        return KeyboardKey.Unknown;
    }
}