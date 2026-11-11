// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Events;

namespace KappaDuck.Khaos.Allocations.Tests.Events;

internal readonly record struct CaptainNamed(string Name);

internal readonly record struct CargoLoaded(int Crates);

internal readonly record struct ShipDocked(int Pier);

[CustomEvents(typeof(CaptainNamed), typeof(CargoLoaded), typeof(ShipDocked))]
internal readonly partial struct HarborEvents;
