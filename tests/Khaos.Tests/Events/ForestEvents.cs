// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;

namespace KappaDuck.Khaos.Tests.Events;

internal readonly record struct NestFound(int Eggs);

internal readonly record struct RangerNamed(string Name);

internal readonly record struct TreeFelled(int Logs);

[CustomEvents(typeof(NestFound), typeof(RangerNamed), typeof(TreeFelled))]
internal readonly partial struct ForestEvents;
