// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using KappaDuck.Khaos.Analyzers.Shared.Unions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;

namespace KappaDuck.Khaos.Analyzers.Events;

[Generator(LanguageNames.CSharp)]
public sealed class CustomEventsGenerator : IIncrementalGenerator
{
    private const string CustomEventType = "global::KappaDuck.Khaos.Events.CustomEvent";
    private const string MemorySafetyFeature = "updated-memory-safety-rules";

    internal const string GroupsTracking = "CustomEventGroups";
    internal const string SourcesTracking = "CustomEventSources";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<CustomEventGroup> groups = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                CustomEventRules.AttributeName,
                static (node, _) => node is StructDeclarationSyntax,
                static (syntax, cancellationToken) => CustomEventRules.Read((INamedTypeSymbol)syntax.TargetSymbol, syntax.SemanticModel.Compilation, cancellationToken))
            .Where(static group => group is not null)
            .Select(static (group, _) => group!)
            .WithTrackingName(GroupsTracking);

        IncrementalValueProvider<bool> memorySafety = context.ParseOptionsProvider
            .Select(static (options, _) => options.Features.ContainsKey(MemorySafetyFeature));

        IncrementalValuesProvider<(CustomEventGroup Group, bool SafeField)> sources = groups
            .Combine(memorySafety)
            .WithTrackingName(SourcesTracking);

        context.RegisterSourceOutput(sources, static (output, source) => output.AddSource(HintName(source.Group), Emit(source.Group, source.SafeField)));
    }

    internal static string HintName(CustomEventGroup group)
        => group.Namespace is null ? $"{group.Name}.CustomEvents.g.cs" : $"{group.Namespace}.{group.Name}.CustomEvents.g.cs";

    private static string Emit(CustomEventGroup group, bool safeField)
    {
        ReadOnlySpan<CustomEventCase> cases = group.Cases.AsSpan();
        UnionCase[] union = new UnionCase[cases.Length];

        for (int i = 0; i < cases.Length; i++)
            union[i] = new UnionCase(cases[i].TypeName, cases[i].Name, cases[i].Managed);

        SourceBuilder builder = new();

        if (group.Namespace is not null)
        {
            builder.AppendLine($"namespace {group.Namespace};")
                   .AppendLine();
        }

        builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (builder.AppendBlock($"{group.Accessibility} readonly partial struct {group.Name}"))
        {
            UnionEmitter.EmitMembers(builder, group.Name, union);

            EmitFrom(builder, group, cases);
            UnionEmitter.EmitStorage(builder, union, safeField);
        }

        builder.AppendLine();
        EmitExtension(builder, group);

        return builder.Build();
    }

    private static void EmitExtension(SourceBuilder builder, CustomEventGroup group)
    {
        builder.AppendLine("/// <summary>")
               .AppendLine($"/// Reads a <see cref=\"{Cref(group.TypeName)}\"/> from a custom event.")
               .AppendLine("/// </summary>");

        using (builder.AppendBlock($"{group.Accessibility} static class {group.Name}Extensions"))
        {
            using (builder.AppendBlock($"extension(in {CustomEventType} e)"))
            {
                builder.AppendLine("/// <summary>")
                       .AppendLine($"/// Gets the event as a <see cref=\"{Cref(group.TypeName)}\"/>, which holds no value when the event is none of its events.")
                       .AppendLine("/// </summary>")
                       .AppendLine($"public {group.TypeName} {group.Name} => {group.TypeName}.From(in e);");
            }
        }
    }

    private static void EmitFrom(SourceBuilder builder, CustomEventGroup group, ReadOnlySpan<CustomEventCase> cases)
    {
        builder.AppendLine()
               .AppendLine("/// <summary>")
               .AppendLine($"/// Reads a custom event as a <see cref=\"{Cref(group.TypeName)}\"/>.")
               .AppendLine("/// </summary>")
               .AppendLine("/// <param name=\"e\">The custom event that <c>OnEvent</c> received.</param>")
               .AppendLine("/// <returns>The event it carries, or a value that holds none when it carries none of the events of this group.</returns>");

        using (builder.AppendBlock($"public static {group.TypeName} From(in {CustomEventType} e)"))
        {
            for (int i = 0; i < cases.Length; i++)
            {
                string local = "value" + (i + 1).ToString(CultureInfo.InvariantCulture);

                builder.AppendLine($"if (e.TryGet(out {cases[i].TypeName} {local}))")
                       .AppendLine($"    return new {group.TypeName}({local});")
                       .AppendLine();
            }

            builder.AppendLine("return default;");
        }
    }

    private static string Cref(string typeName) => typeName.StartsWith("global::", StringComparison.Ordinal) ? typeName["global::".Length..] : typeName;
}
