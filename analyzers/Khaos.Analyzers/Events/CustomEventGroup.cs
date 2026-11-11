// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;

namespace KappaDuck.Khaos.Analyzers.Events;

internal sealed record CustomEventGroup(string Name, string? Namespace, string TypeName, string Accessibility, EquatableArray<CustomEventCase> Cases);
