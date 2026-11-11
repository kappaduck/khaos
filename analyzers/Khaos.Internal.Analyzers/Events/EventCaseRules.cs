// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

internal static class EventCaseRules
{
    private const string ContextName = "KappaDuck.Khaos.Events.EventContext";
    private const string NativeEventName = "KappaDuck.Khaos.Interop.SDL.Primitives.SDL_Event";

    internal const string AttributeName = "KappaDuck.Khaos.Events.KhaosEventAttribute";

    internal static EventCaseProblem? Check(INamedTypeSymbol type, AttributeData attribute, Compilation compilation, CancellationToken cancellationToken, out EventCase? @case)
    {
        @case = null;

        if (!IsPublicReadOnlyRecordStruct(type))
            return new EventCaseProblem(Descriptors.EventCaseShape, type.Name);

        if (!TryReadRange(attribute, out NativeType first, out NativeType last))
            return null;

        if (last.Value < first.Value)
            return new EventCaseProblem(Descriptors.EventCaseRange, type.Name);

        if (FindReferenceField(type) is { } field)
            return new EventCaseProblem(Descriptors.EventCaseField, type.Name, field.AssociatedSymbol?.Name ?? field.Name, field.Type.ToDisplayString());

        EventCaseProblem? problem = CheckConstructor(type, compilation, cancellationToken, out string? nativeField, out bool takesContext);

        if (problem is not null)
            return problem;

        string typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        @case = new EventCase(type.Name, typeName, first.Value, last.Value, first.Label, last.Label, nativeField, takesContext, !type.IsUnmanagedType);

        return null;
    }

    internal static EventCase? Read(ISymbol symbol, AttributeData attribute, Compilation compilation, CancellationToken cancellationToken)
    {
        _ = Check((INamedTypeSymbol)symbol, attribute, compilation, cancellationToken, out EventCase? @case);
        return @case;
    }

    private static EventCaseProblem? CheckConstructor(INamedTypeSymbol type, Compilation compilation, CancellationToken cancellationToken, out string? nativeField, out bool takesContext)
    {
        nativeField = null;
        takesContext = false;

        List<IMethodSymbol> declared = [];

        foreach (IMethodSymbol constructor in type.InstanceConstructors)
        {
            if (!constructor.IsImplicitlyDeclared && !IsPositional(constructor, cancellationToken))
                declared.Add(constructor);
        }

        if (declared.Count == 0)
            return HasInstanceFields(type) ? Constructor(type, "has a payload but no internal constructor that takes its native event") : null;

        if (declared.Count > 1)
            return Constructor(type, "declares more than one constructor besides its positional one");

        IMethodSymbol native = declared[0];

        if (native.DeclaredAccessibility != Accessibility.Internal || !HasNativeParameters(native, compilation, out takesContext))
            return Constructor(type, "must declare its native constructor internal, taking 'in' the type of one SDL_Event field, then optionally 'in EventContext'");

        ITypeSymbol payload = native.Parameters[0].Type;
        IFieldSymbol[] fields = FieldsOfType(compilation, payload);

        if (fields.Length != 1)
            return Constructor(type, $"takes {payload.Name}, which is not the type of exactly one SDL_Event field");

        nativeField = fields[0].Name;
        return null;

        static bool IsPositional(IMethodSymbol constructor, CancellationToken cancellationToken)
            => constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax);

        static bool HasInstanceFields(INamedTypeSymbol type)
            => type.GetMembers().OfType<IFieldSymbol>().Any(static field => !field.IsStatic && !field.IsConst);
    }

    private static EventCaseProblem Constructor(INamedTypeSymbol type, string reason) => new(Descriptors.EventCaseConstructor, type.Name, reason);

    private static IFieldSymbol[] FieldsOfType(Compilation compilation, ITypeSymbol payload)
    {
        INamedTypeSymbol? native = compilation.GetTypeByMetadataName(NativeEventName);

        if (native is null)
            return [];

        return
        [
            .. native.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(field => !field.IsStatic && SymbolEqualityComparer.Default.Equals(field.Type, payload))
        ];
    }

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

    private static bool HasNativeParameters(IMethodSymbol constructor, Compilation compilation, out bool takesContext)
    {
        takesContext = false;

        if (constructor.Parameters is [{ RefKind: RefKind.In }])
            return true;

        if (constructor.Parameters is [{ RefKind: RefKind.In }, { RefKind: RefKind.In } context])
        {
            INamedTypeSymbol? expected = compilation.GetTypeByMetadataName(ContextName);
            takesContext = expected is not null && SymbolEqualityComparer.Default.Equals(context.Type, expected);

            return takesContext;
        }

        return false;
    }

    private static bool IsPublicReadOnlyRecordStruct(INamedTypeSymbol type) => type is
    {
        TypeKind: TypeKind.Struct,
        IsRecord: true,
        IsReadOnly: true,
        IsGenericType: false,
        ContainingType: null,
        DeclaredAccessibility: Accessibility.Public
    };

    private static bool TryReadRange(AttributeData attribute, out NativeType first, out NativeType last)
    {
        first = default;
        last = default;

        if (attribute.ConstructorArguments is [TypedConstant single])
        {
            if (!TryReadNativeType(single, out first))
                return false;

            last = first;
            return true;
        }

        if (attribute.ConstructorArguments is [TypedConstant from, TypedConstant to])
            return TryReadNativeType(from, out first) && TryReadNativeType(to, out last);

        return false;
    }

    private static bool TryReadNativeType(TypedConstant constant, out NativeType nativeType)
    {
        nativeType = default;

        if (constant is not { Kind: TypedConstantKind.Enum, Type: INamedTypeSymbol enumType, Value: not null })
            return false;

        uint value = Convert.ToUInt32(constant.Value, CultureInfo.InvariantCulture);
        string typeName = enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        IFieldSymbol[] members =
        [
            .. enumType.GetMembers()
                .OfType<IFieldSymbol>()
                .Where(member => member.HasConstantValue && Convert.ToUInt32(member.ConstantValue, CultureInfo.InvariantCulture) == value)
        ];

        string label = members is [IFieldSymbol member]
            ? $"{typeName}.{member.Name}"
            : $"({typeName})0x{value.ToString("X", CultureInfo.InvariantCulture)}";

        nativeType = new NativeType(value, label);
        return true;
    }

    private readonly record struct NativeType(uint Value, string Label);
}
