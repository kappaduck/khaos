// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Events.Helpers;
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

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<CustomEventGroup> groups = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                CustomEventRules.AttributeName,
                static (node, _) => node is StructDeclarationSyntax,
                static (syntax, ct) => CustomEventRules.Read((INamedTypeSymbol)syntax.TargetSymbol, syntax.SemanticModel.Compilation, ct))
            .Where(static group => group is not null)
            .Select(static (group, _) => group!)
            .WithTrackingName(GroupsTracking);

        IncrementalValueProvider<bool> memorySafety = context.ParseOptionsProvider
            .Select(static (options, _) => options.Features.ContainsKey(MemorySafetyFeature));

        IncrementalValuesProvider<(CustomEventGroup Group, bool MemorySafety)> sources = groups
            .Combine(memorySafety)
            .WithTrackingName(SourcesTracking);

        context.RegisterSourceOutput(sources, static (context, source) => context.AddSource(HintName(source.Group), Emit(source.Group, source.MemorySafety)));
    }

    private static string HintName(CustomEventGroup group)
        => group.Namespace is null ? $"{group.Name}.CustomEvents.g.cs" : $"{group.Namespace}.{group.Name}.CustomEvents.g.cs";

    private static string Emit(CustomEventGroup group, bool memorySafety)
    {
        ReadOnlySpan<UnionCase> cases = group.Cases.AsSpan();

        using CodeSourceBuilder builder = new();

        if (group.Namespace is not null)
        {
            builder.AppendLine($"namespace {group.Namespace};")
                   .AppendLine();
        }

        builder.AppendLine("[global::System.Runtime.CompilerServices.Union]");

        using (builder.AppendBlock($"{group.Accessibility} readonly partial struct {Identifiers.Escape(group.Name)}"))
        {
            UnionEmitter.EmitMembers(builder, Identifiers.Escape(group.Name), cases);

            EmitFrom(builder, group, cases);
            UnionEmitter.EmitToString(builder, group.Name, cases);
            UnionEmitter.EmitStorage(builder, cases, memorySafety);
        }

        builder.AppendLine();
        EmitExtension(builder, group);

        return builder.ToString();
    }

    private static void EmitFrom(CodeSourceBuilder builder, CustomEventGroup group, ReadOnlySpan<UnionCase> cases)
    {
        builder.AppendLine()
               .AppendLine("/// <summary>")
               .AppendLine($"/// Reads a custom event as a <see cref=\"{UnionEmitter.Cref(group.TypeName)}\"/>.")
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

    private static void EmitExtension(CodeSourceBuilder builder, CustomEventGroup group)
    {
        builder.AppendLine("/// <summary>")
               .AppendLine($"/// Reads a <see cref=\"{UnionEmitter.Cref(group.TypeName)}\"/> from a custom event.")
               .AppendLine("/// </summary>");

        using (builder.AppendBlock($"{group.Accessibility} static class {group.Name}Extensions"))
        {
            using (builder.AppendBlock($"extension(in {CustomEventType} e)"))
            {
                builder.AppendLine("/// <summary>")
                       .AppendLine($"/// Gets the event as a <see cref=\"{UnionEmitter.Cref(group.TypeName)}\"/>, which holds no value when the event is none of its events.")
                       .AppendLine("/// </summary>")
                       .AppendLine($"public {group.TypeName} {Identifiers.Escape(group.Name)} => {group.TypeName}.From(in e);");
            }
        }
    }
}
