// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Internal.Analyzers.Tests.Events;

internal static class EventSources
{
    internal const string Framework = """
        using System;
        using System.Diagnostics;
        using System.Runtime.InteropServices;
        using KappaDuck.Khaos.Interop.SDL.Primitives;

        namespace KappaDuck.Khaos.Interop.SDL.Primitives
        {
            internal enum SDL_EventType : uint
            {
                Quit = 0x100,
                WindowResized = 0x206,
                KeyDown = 0x300,
                KeyUp = 0x301,
                User = 0x8000,
                Last = 0xFFFF
            }

            internal struct SDL_WindowEvent
            {
                public SDL_EventType Type;
                public int Data1;
                public int Data2;
            }

            internal struct SDL_KeyboardEvent
            {
                public SDL_EventType Type;
                public int Scancode;
            }

            internal struct SDL_UserEvent
            {
                public SDL_EventType Type;
                public int Code;
            }

            [StructLayout(LayoutKind.Explicit, Size = 128)]
            internal struct SDL_Event
            {
                [FieldOffset(0)]
                public safe SDL_EventType Type;

                [FieldOffset(0)]
                public safe SDL_WindowEvent Window;

                [FieldOffset(0)]
                public safe SDL_KeyboardEvent Key;

                [FieldOffset(0)]
                public safe SDL_UserEvent User;
            }
        }

        namespace KappaDuck.Khaos.Events
        {
            [AttributeUsage(AttributeTargets.Struct, Inherited = false)]
            [Conditional("KHAOS_EVENT_METADATA")]
            internal sealed class KhaosEventAttribute : Attribute
            {
                internal KhaosEventAttribute(SDL_EventType type)
                {
                }

                internal KhaosEventAttribute(SDL_EventType first, SDL_EventType last)
                {
                }
            }

            internal readonly struct EventContext(float contentScale)
            {
                internal float ContentScale { get; } = contentScale;
            }
        }
        """;

    internal const string Event = """
        namespace KappaDuck.Khaos.Events;

        public readonly partial struct Event;
        """;

    internal const string Cases = """
        using KappaDuck.Khaos.Interop.SDL.Primitives;

        namespace KappaDuck.Khaos.Events;

        [KhaosEvent(SDL_EventType.Quit)]
        public readonly record struct QuitRequested;

        [KhaosEvent(SDL_EventType.WindowResized)]
        public readonly record struct WindowResized(int Width, int Height)
        {
            internal WindowResized(in SDL_WindowEvent native, in EventContext context)
                : this((int)(native.Data1 / context.ContentScale), (int)(native.Data2 / context.ContentScale))
            {
            }
        }

        [KhaosEvent(SDL_EventType.KeyDown)]
        public readonly record struct KeyPressed(int Code)
        {
            internal KeyPressed(in SDL_KeyboardEvent native)
                : this(native.Scancode)
            {
            }
        }

        [KhaosEvent(SDL_EventType.User, SDL_EventType.Last)]
        public readonly record struct CustomEvent
        {
            internal CustomEvent(in SDL_UserEvent native) => Code = native.Code;

            internal int Code { get; }
        }
        """;
}
