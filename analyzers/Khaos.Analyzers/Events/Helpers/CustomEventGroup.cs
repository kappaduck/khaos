// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using KappaDuck.Khaos.Analyzers.Shared.Unions;

namespace KappaDuck.Khaos.Analyzers.Events.Helpers;

internal sealed record CustomEventGroup(string Name, string? Namespace, string TypeName, string Accessibility, EquatableArray<UnionCase> Cases);
