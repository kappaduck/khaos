// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Analyzers.Tests.Fixtures;

internal static class HostingSources
{
    internal const string Framework = """
        namespace KappaDuck.Khaos.Windowing
        {
            public sealed class Window
            {
                public string Title { get; set; } = "";
            }
        }

        namespace KappaDuck.Khaos
        {
            public abstract class Game2D
            {
                protected Game2D(int width, int height)
                {
                }

                public KappaDuck.Khaos.Windowing.Window Window => null!;

                protected virtual void Initialize()
                {
                }
            }
        }
        """;
}
