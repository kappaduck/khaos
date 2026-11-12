// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Analyzers.Tests.Fixtures;

internal static class CustomEventSources
{
    internal const string Events = """
        using System;

        namespace Launcher;

        public readonly record struct LevelCompleted(int Level, TimeSpan Time);

        public readonly record struct DownloadFinished(int Version);

        public readonly record struct PlayerRenamed(string Name);
        """;

    internal const string Framework = """
        using System;
        using System.Diagnostics;

        namespace KappaDuck.Khaos.Events;

        [AttributeUsage(AttributeTargets.Struct, Inherited = false)]
        [Conditional("KHAOS_CUSTOM_EVENTS_METADATA")]
        public sealed class CustomEventsAttribute(params Type[] events) : Attribute
        {
            public Type[] Events { get; } = events;
        }

        public readonly record struct CustomEvent
        {
            public bool Is<T>() where T : struct => false;

            public bool TryGet<T>(out T value) where T : struct
            {
                value = default;
                return false;
            }
        }

        [System.Runtime.CompilerServices.Union]
        public readonly struct Event
        {
            private readonly CustomEvent _custom;
            private readonly bool _hasValue;

            public Event(CustomEvent value)
            {
                _custom = value;
                _hasValue = true;
            }

            public bool HasValue => _hasValue;

            public object? Value => _hasValue ? _custom : null;

            public bool TryGetValue(out CustomEvent value)
            {
                value = _custom;
                return _hasValue;
            }
        }
        """;

    internal const string Group = """
        using System;
        using KappaDuck.Khaos.Events;

        namespace Launcher;

        public readonly record struct LevelCompleted(int Level, TimeSpan Time);

        public readonly record struct DownloadFinished(int Version);

        public readonly record struct PlayerRenamed(string Name);

        [CustomEvents(typeof(LevelCompleted), typeof(DownloadFinished), typeof(PlayerRenamed))]
        public readonly partial struct LauncherEvents;
        """;

    internal const string GroupOnly = """
        using KappaDuck.Khaos.Events;

        namespace Launcher;

        [CustomEvents(typeof(LevelCompleted), typeof(DownloadFinished), typeof(PlayerRenamed))]
        public readonly partial struct LauncherEvents;
        """;
}
