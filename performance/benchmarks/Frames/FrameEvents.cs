// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;

namespace KappaDuck.Khaos.Benchmarks.Frames;

internal readonly record struct ScoreChanged(int Points);

[CustomEvents(typeof(ScoreChanged))]
internal readonly partial struct FrameEvents;
