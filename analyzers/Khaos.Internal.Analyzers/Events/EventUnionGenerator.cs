// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Analyzers.Shared;
using KappaDuck.Khaos.Analyzers.Shared.Unions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KappaDuck.Khaos.Internal.Analyzers.Events;

[Generator(LanguageNames.CSharp)]
public sealed class EventUnionGenerator : IIncrementalGenerator
{
    private const string ContextType = "global::KappaDuck.Khaos.Events.EventContext";
    private const string Namespace = "KappaDuck.Khaos.Events";
    private const string NativeEventType = "global::KappaDuck.Khaos.Interop.SDL.Primitives.SDL_Event";

    internal const string CasesTracking = "EventCases";
    internal const string ConverterHintName = "KappaDuck.Khaos.Events.EventConverter.g.cs";
    internal const string EventHintName = "KappaDuck.Khaos.Events.Event.g.cs";
    internal const string UnionTracking = "EventUnion";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EventCase> cases = context.SyntaxProvider
            .ForAttributeWithMetadataName(
            EventCaseRules.AttributeName,
            static (node, _) => node is TypeDeclarationSyntax,
            static (syntax, cancellationToken) => EventCaseRules.Read(syntax.TargetSymbol, syntax.Attributes[0], syntax.SemanticModel.Compilation, cancellationToken))
            .Where(static c => c is not null)
            .Select(static (@case, _) => @case!)
            .WithTrackingName(CasesTracking);

        IncrementalValueProvider<EquatableArray<EventCase>> union = cases
            .Collect()
            .Select(static (all, _) => EventUnion.Resolve(all))
            .WithTrackingName(UnionTracking);

        context.RegisterSourceOutput(union, static (output, all) => Emit(output, all));
    }

    private static void Emit(SourceProductionContext output, EquatableArray<EventCase> all)
    {
        if (all.IsEmpty)
            return;

        output.AddSource(EventHintName, EmitEvent(all.AsSpan()));
        output.AddSource(ConverterHintName, EmitConverter(all.AsSpan()));
    }

    private static string EmitConverter(ReadOnlySpan<EventCase> cases)
    {
        SourceBuilder builder = new();

        builder.AppendLine($"namespace {Namespace};")
               .AppendLine();

        using (builder.AppendBlock("internal static class EventConverter"))
        {
            using (builder.AppendBlock($"internal static bool TryConvert(in {NativeEventType} native, in {ContextType} context, out Event e)"))
            {
                using (builder.AppendBlock("switch (native.Type)"))
                {
                    foreach (EventCase @case in cases)
                    {
                        builder.AppendLine(Label(@case))
                               .AppendLine($"    e = new Event(new {@case.TypeName}({Arguments(@case)}));")
                               .AppendLine("    return true;")
                               .AppendLine();
                    }

                    builder.AppendLine("default:")
                           .AppendLine("    e = default;")
                           .AppendLine("    return false;");
                }
            }
        }

        return builder.Build();

        static string Arguments(EventCase @case)
        {
            if (@case.NativeField is null)
                return string.Empty;

            return @case.TakesContext ? $"in native.{@case.NativeField}, in context" : $"in native.{@case.NativeField}";
        }

        static string Label(EventCase @case)
            => @case.IsRange ? $"case >= {@case.FirstLabel} and <= {@case.LastLabel}:" : $"case {@case.FirstLabel}:";
    }

    private static string EmitEvent(ReadOnlySpan<EventCase> cases)
    {
        UnionCase[] union = new UnionCase[cases.Length];

        for (int i = 0; i < cases.Length; i++)
            union[i] = new UnionCase(cases[i].TypeName, cases[i].Name, cases[i].Managed);

        SourceBuilder builder = new();

        builder.AppendLine($"namespace {Namespace};")
               .AppendLine();

        using (builder.AppendBlock("public readonly partial struct Event"))
        {
            UnionEmitter.EmitMembers(builder, "Event", union);
            UnionEmitter.EmitStorage(builder, union, true);
        }

        return builder.Build();
    }
}
