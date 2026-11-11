// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace KappaDuck.Khaos.Analyzers;

internal static class Descriptors
{
    private const string Documentation = "https://github.com/kappaduck/khaos/blob/main/docs/rules/";
    private const string EventDuplicateDescription = "The group holds each event under the name of the event, so every event is listed once and has a name that no other event of the group has.";
    private const string EventGroupEmptyDescription = "A group with no events gives OnEvent nothing to match. List at least one readonly record struct in the attribute.";
    private const string EventGroupNameDescription = "OnEvent reads a group through a property of CustomEvent named after the group. When CustomEvent already has a member with that name, the pattern binds to that member instead, and the events of the group never match.";
    private const string EventGroupShapeDescription = "Khaos adds the members that let OnEvent match the events of the group to the group itself, so it has to be a partial struct it can extend, readonly like every event, at the top level of a namespace and not generic.";
    private const string EventShapeDescription = "Each event of a group is copied into the group and read from OnEvent, so it has to be an immutable value: a non-generic readonly record struct, at least as accessible as the group, that holds values and strings, not other objects.";
    private const string Events = "Events";

    internal static readonly DiagnosticDescriptor EventGroupShape = new(
        "KHAOS001",
        "[CustomEvents] is not on a readonly partial struct",
        "'{0}' is marked [CustomEvents]; declare it as a readonly partial struct, not generic and not nested in another type",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventGroupShapeDescription,
        $"{Documentation}khaos001.md");

    internal static readonly DiagnosticDescriptor EventGroupTooLarge = new(
        "KHAOS001",
        "[CustomEvents] is not on a readonly partial struct",
        "'{0}' holds {1} events; a group holds at most 255, so split it in two",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventGroupShapeDescription,
        $"{Documentation}khaos001.md");

    internal static readonly DiagnosticDescriptor EventGroupEmpty = new(
        "KHAOS002",
        "[CustomEvents] group has no events",
        "'{0}' is marked [CustomEvents] but lists no events; list them as [CustomEvents(typeof(MyEvent))]",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventGroupEmptyDescription,
        $"{Documentation}khaos002.md");

    internal static readonly DiagnosticDescriptor EventShape = new(
        "KHAOS003",
        "event in a [CustomEvents] group has the wrong shape",
        "'{0}' in '{1}' {2}",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventShapeDescription,
        $"{Documentation}khaos003.md");

    internal static readonly DiagnosticDescriptor EventGroupName = new(
        "KHAOS004",
        "[CustomEvents] group is named like a member of CustomEvent",
        "'{0}' has the name of a member of CustomEvent, so 'CustomEvent {{ {0}: … }}' reads that member instead of the group; rename the group",
        Events,
        DiagnosticSeverity.Warning,
        true,
        EventGroupNameDescription,
        $"{Documentation}khaos004.md");

    internal static readonly DiagnosticDescriptor EventDuplicate = new(
        "KHAOS005",
        "event in a [CustomEvents] group is listed twice or shares a name",
        "'{0}' in '{1}' {2}",
        Events,
        DiagnosticSeverity.Error,
        true,
        EventDuplicateDescription,
        $"{Documentation}khaos005.md");
}
