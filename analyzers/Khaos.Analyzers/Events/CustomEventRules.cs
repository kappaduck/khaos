// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Events;

internal static class CustomEventRules
{
    internal const string AttributeName = "KappaDuck.Khaos.Events.CustomEventsAttribute";

    private const string CustomEventName = "KappaDuck.Khaos.Events.CustomEvent";
    private const int MaxEvents = 255;
    private const string StorageName = "Cases";

    internal static CustomEventGroup? Check(INamedTypeSymbol type, Compilation compilation, List<CustomEventProblem> problems, CancellationToken cancellationToken)
    {
        Location location = type.Locations[0];

        if (!IsReadOnlyPartialStruct(type, cancellationToken))
        {
            problems.Add(new CustomEventProblem(Descriptors.EventGroupShape, location, type.Name));
            return null;
        }

        AttributeData? attribute = FindAttribute(type);
        ImmutableArray<TypedConstant> listed = ListedEvents(attribute);
        AttributeArgumentListSyntax? arguments = (attribute?.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax)?.ArgumentList;
        bool oneArgumentPerEvent = arguments is not null && arguments.Arguments.Count == listed.Length;

        List<CustomEventCase> cases = [];
        HashSet<ISymbol> seen = [with(SymbolEqualityComparer.Default)];
        Dictionary<string, string> names = [with(StringComparer.Ordinal)];
        int rejected = 0;

        for (int i = 0; i < listed.Length; i++)
        {
            if (listed[i].Value is not ITypeSymbol listedType || listedType.TypeKind == TypeKind.Error)
            {
                rejected++;
                continue;
            }

            Location at = oneArgumentPerEvent ? arguments!.Arguments[i].GetLocation() : location;
            string fullName = listedType.ToDisplayString();

            if (!seen.Add(listedType))
            {
                rejected++;
                problems.Add(new CustomEventProblem(Descriptors.EventDuplicate, at, fullName, type.Name, "is listed twice; list it once"));
                continue;
            }

            string? reason = CheckEvent(listedType, type.DeclaredAccessibility);

            if (reason is not null)
            {
                rejected++;
                problems.Add(new CustomEventProblem(Descriptors.EventShape, at, listedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), type.Name, reason));
                continue;
            }

            if (names.TryGetValue(listedType.Name, out string? other))
            {
                rejected++;
                problems.Add(new CustomEventProblem(Descriptors.EventDuplicate, at, fullName, type.Name, $"has the same name as '{other}'; rename one of them"));
                continue;
            }

            names.Add(listedType.Name, fullName);
            cases.Add(new CustomEventCase(listedType.Name, listedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), !listedType.IsUnmanagedType));
        }

        if (cases.Count == 0 && rejected == 0)
            problems.Add(new CustomEventProblem(Descriptors.EventGroupEmpty, location, type.Name));

        if (cases.Count > MaxEvents)
            problems.Add(new CustomEventProblem(Descriptors.EventGroupTooLarge, location, type.Name, cases.Count));

        if (IsNamedLikeACustomEventMember(type.Name, compilation))
            problems.Add(new CustomEventProblem(Descriptors.EventGroupName, location, type.Name));

        string? @namespace = type.ContainingNamespace is { IsGlobalNamespace: false } containing ? containing.ToDisplayString() : null;
        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string accessibility = type.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";

        return new CustomEventGroup(type.Name, @namespace, typeName, accessibility, cases.ToEquatableArray());
    }

    internal static CustomEventGroup? Read(INamedTypeSymbol type, Compilation compilation, CancellationToken cancellationToken)
    {
        List<CustomEventProblem> problems = [];
        CustomEventGroup? group = Check(type, compilation, problems, cancellationToken);

        return problems.Exists(static problem => problem.Descriptor.DefaultSeverity == DiagnosticSeverity.Error) ? null : group;
    }

    private static string? CheckEvent(ITypeSymbol listed, Accessibility group)
    {
        if (listed is not INamedTypeSymbol { TypeKind: TypeKind.Struct, IsRecord: true, IsReadOnly: true, IsGenericType: false } type)
            return "is not a non-generic readonly record struct";

        if (!IsVisibleFrom(type, group))
        {
            return group == Accessibility.Public
                ? "is less accessible than the group; make it public, or make the group internal"
                : "must be public or internal, so that OnEvent can match it";
        }

        if (string.Equals(type.Name, StorageName, StringComparison.Ordinal))
            return "has a name that the group uses for its own members; rename the event";

        if (FindReferenceField(type) is { } field)
            return $"holds '{field.AssociatedSymbol?.Name ?? field.Name}' of type {field.Type.ToDisplayString()}; an event holds only values and strings";

        return null;
    }

    private static AttributeData? FindAttribute(INamedTypeSymbol type)
        => type.GetAttributes().FirstOrDefault(static attribute => string.Equals(attribute.AttributeClass?.ToDisplayString(), AttributeName, StringComparison.Ordinal));

    private static IFieldSymbol? FindReferenceField(INamedTypeSymbol type)
    {
        if (type.IsUnmanagedType)
            return null;

        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.IsConst)
                continue;

            if (field.Type.SpecialType != SpecialType.System_String && !field.Type.IsUnmanagedType)
                return field;
        }

        return null;
    }

    private static bool IsNamedLikeACustomEventMember(string name, Compilation compilation)
    {
        if (string.Equals(name, "GetType", StringComparison.Ordinal))
            return true;

        INamedTypeSymbol? customEvent = compilation.GetTypeByMetadataName(CustomEventName);

        return customEvent?.GetMembers(name).Any(static member => member.DeclaredAccessibility == Accessibility.Public) == true;
    }

    private static bool IsReadOnlyPartialStruct(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        if (type is not { TypeKind: TypeKind.Struct, IsReadOnly: true, IsRecord: false, IsGenericType: false, ContainingType: null })
            return false;

        if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            return false;

        return type.DeclaringSyntaxReferences.All(reference => reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword));
    }

    private static bool IsVisibleFrom(INamedTypeSymbol type, Accessibility group)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility == Accessibility.Public)
                continue;

            if (group == Accessibility.Internal && current.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal)
                continue;

            return false;
        }

        return true;
    }

    private static ImmutableArray<TypedConstant> ListedEvents(AttributeData? attribute)
    {
        if (attribute is null || attribute.ConstructorArguments.Length != 1)
            return [];

        TypedConstant events = attribute.ConstructorArguments[0];

        return events.Kind == TypedConstantKind.Array && !events.IsNull ? events.Values : [];
    }
}
