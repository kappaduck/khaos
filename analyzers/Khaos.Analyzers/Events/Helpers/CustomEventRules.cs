// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using KappaDuck.Khaos.Analyzers.Shared.Unions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace KappaDuck.Khaos.Analyzers.Events.Helpers;

internal static class CustomEventRules
{
    internal const string AttributeName = "KappaDuck.Khaos.Events.CustomEventsAttribute";

    private const string CustomEventName = "KappaDuck.Khaos.Events.CustomEvent";
    private const int MaxEvents = 255;
    private const string StorageName = "Cases";

    internal static CustomEventGroup? Check(INamedTypeSymbol type, Compilation compilation, List<Diagnostic> problems, CancellationToken cancellationToken)
    {
        AttributeData? attribute = FindAttribute(type);
        AttributeSyntax? syntax = attribute?.ApplicationSyntaxReference?.GetSyntax(cancellationToken) as AttributeSyntax;
        Location location = syntax?.FirstAncestorOrSelf<TypeDeclarationSyntax>()?.Identifier.GetLocation() ?? type.Locations[0];

        if (!IsReadOnlyPartialStruct(type, cancellationToken))
        {
            problems.Add(Diagnostic.Create(Descriptors.EventGroupShape, location, type.Name));
            return null;
        }

        if (attribute is not { AttributeConstructor: not null })
            return null;

        ImmutableArray<TypedConstant> listed = ListedEvents(attribute);
        AttributeArgumentListSyntax? arguments = syntax?.ArgumentList;
        bool oneArgumentPerEvent = arguments is not null && arguments.Arguments.Count == listed.Length;

        List<UnionCase> cases = [];
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
                problems.Add(Diagnostic.Create(Descriptors.EventDuplicate, at, fullName, type.Name, "is listed twice; list it once"));
                continue;
            }

            string? reason = CheckEvent(listedType, type.DeclaredAccessibility);

            if (reason is not null)
            {
                rejected++;
                problems.Add(Diagnostic.Create(Descriptors.EventShape, at, listedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), type.Name, reason));
                continue;
            }

            if (names.TryGetValue(listedType.Name, out string? other))
            {
                rejected++;
                problems.Add(Diagnostic.Create(Descriptors.EventDuplicate, at, fullName, type.Name, $"has the same name as '{other}'; rename one of them"));
                continue;
            }

            names.Add(listedType.Name, fullName);
            cases.Add(new UnionCase(listedType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), listedType.Name, !listedType.IsUnmanagedType));
        }

        if (cases.Count == 0 && rejected == 0)
            problems.Add(Diagnostic.Create(Descriptors.EventGroupEmpty, location, type.Name));

        if (cases.Count > MaxEvents)
            problems.Add(Diagnostic.Create(Descriptors.EventGroupTooLarge, location, type.Name, cases.Count));

        if (IsNamedLikeACustomEventMember(type.Name, compilation))
            problems.Add(Diagnostic.Create(Descriptors.EventGroupName, location, type.Name));

        string? @namespace = type.ContainingNamespace is { IsGlobalNamespace: false } containing ? containing.ToDisplayString() : null;
        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string accessibility = type.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";

        return new CustomEventGroup(type.Name, @namespace, typeName, accessibility, cases.ToEquatableArray());
    }

    internal static CustomEventGroup? Read(INamedTypeSymbol type, Compilation compilation, CancellationToken cancellationToken)
    {
        List<Diagnostic> problems = [];
        CustomEventGroup? group = Check(type, compilation, problems, cancellationToken);

        if (group is not { Cases.IsEmpty: false })
            return null;

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

        if (UnionCaseRules.FindReferenceField(type) is { } field)
            return $"holds '{field.AssociatedSymbol?.Name ?? field.Name}' of type {field.Type.ToDisplayString()}; an event holds only values and strings";

        return null;
    }

    private static AttributeData? FindAttribute(INamedTypeSymbol type)
        => type.GetAttributes().FirstOrDefault(static attribute => string.Equals(attribute.AttributeClass?.ToDisplayString(), AttributeName, StringComparison.Ordinal));

    private static bool IsNamedLikeACustomEventMember(string name, Compilation compilation)
    {
        INamedTypeSymbol? customEvent = compilation.GetTypeByMetadataName(CustomEventName);

        return HasPublicMember(customEvent, name) || HasPublicMember(compilation.GetSpecialType(SpecialType.System_Object), name);

        static bool HasPublicMember(INamedTypeSymbol? type, string name)
            => type?.GetMembers(name).Any(static member => member.DeclaredAccessibility == Accessibility.Public) == true;
    }

    private static bool IsReadOnlyPartialStruct(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        if (type is not { TypeKind: TypeKind.Struct, IsReadOnly: true, IsRecord: false, IsGenericType: false, IsFileLocal: false, ContainingType: null })
            return false;

        return type.DeclaringSyntaxReferences.All(reference => reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword));
    }

    private static bool IsVisibleFrom(INamedTypeSymbol type, Accessibility group)
    {
        INamedTypeSymbol? current = type;

        while (current is not null)
        {
            if (current.IsFileLocal)
                return false;

            if (!IsVisible(current, group))
                return false;

            current = current.ContainingType;
        }

        static bool IsVisible(INamedTypeSymbol current, Accessibility accessibility)
        {
            return current.DeclaredAccessibility == Accessibility.Public
                || (accessibility == Accessibility.Internal && current.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal);
        }

        return true;
    }

    private static ImmutableArray<TypedConstant> ListedEvents(AttributeData attribute)
        => attribute.ConstructorArguments is [{ IsNull: false } events] ? events.Values : [];
}
