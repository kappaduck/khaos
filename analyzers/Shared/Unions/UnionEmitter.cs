// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Globalization;

namespace KappaDuck.Khaos.Analyzers.Shared.Unions;

internal static class UnionEmitter
{
    private const string CasesField = "_cases";
    private const string CasesType = "Cases";
    private const string GlobalPrefix = "global::";
    private const string TagField = "_tag";

    internal static void EmitMembers(SourceBuilder builder, string unionName, ReadOnlySpan<UnionCase> cases)
    {
        EmitFields(builder, cases, HasUnmanaged(cases));
        EmitConstructors(builder, unionName, cases);
        EmitHasValue(builder);
        EmitValue(builder, cases);
        EmitTryGetValues(builder, cases);
    }

    internal static void EmitStorage(SourceBuilder builder, ReadOnlySpan<UnionCase> cases, bool safeField)
    {
        if (HasUnmanaged(cases))
            EmitCases(builder, cases, safeField);
    }

    private static string Cref(UnionCase @case)
        => @case.TypeName.StartsWith(GlobalPrefix, StringComparison.Ordinal) ? @case.TypeName[GlobalPrefix.Length..] : @case.TypeName;

    private static void EmitCases(SourceBuilder builder, ReadOnlySpan<UnionCase> cases, bool safeField)
    {
        string modifier = safeField ? "safe " : string.Empty;

        builder.AppendLine()
               .AppendLine("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit)]");

        using (builder.AppendBlock($"private readonly struct {CasesType}"))
        {
            foreach (UnionCase @case in cases)
            {
                if (@case.Managed)
                    continue;

                builder.AppendLine($"internal {CasesType}({@case.TypeName} value) => {@case.MemberName} = value;")
                       .AppendLine();
            }

            bool first = true;

            foreach (UnionCase @case in cases)
            {
                if (@case.Managed)
                    continue;

                if (!first)
                    builder.AppendLine();

                first = false;

                builder.AppendLine("[global::System.Runtime.InteropServices.FieldOffset(0)]")
                       .AppendLine($"internal readonly {modifier}{@case.TypeName} {@case.MemberName};");
            }
        }
    }

    private static void EmitConstructors(SourceBuilder builder, string unionName, ReadOnlySpan<UnionCase> cases)
    {
        for (int i = 0; i < cases.Length; i++)
        {
            UnionCase @case = cases[i];

            builder.AppendLine()
                   .AppendLine("/// <summary>")
                   .AppendLine($"/// Creates an instance that holds a <see cref=\"{Cref(@case)}\"/>.")
                   .AppendLine("/// </summary>")
                   .AppendLine("/// <param name=\"value\">The value to hold.</param>");

            using (builder.AppendBlock($"public {unionName}({@case.TypeName} value)"))
            {
                builder.AppendLine($"{TagField} = {Tag(i)};")
                       .AppendLine(@case.Managed ? $"{ManagedField(@case)} = value;" : $"{CasesField} = new {CasesType}(value);");
            }
        }
    }

    private static void EmitFields(SourceBuilder builder, ReadOnlySpan<UnionCase> cases, bool overlaps)
    {
        builder.AppendLine($"private readonly byte {TagField};");

        if (overlaps)
            builder.AppendLine($"private readonly {CasesType} {CasesField};");

        foreach (UnionCase @case in cases)
        {
            if (@case.Managed)
                builder.AppendLine($"private readonly {@case.TypeName} {ManagedField(@case)};");
        }
    }

    private static void EmitHasValue(SourceBuilder builder)
    {
        builder.AppendLine()
               .AppendLine("/// <summary>")
               .AppendLine("/// Gets a value indicating whether this instance holds a value.")
               .AppendLine("/// </summary>")
               .AppendLine($"public bool HasValue => {TagField} != 0;");
    }

    private static void EmitTryGetValues(SourceBuilder builder, ReadOnlySpan<UnionCase> cases)
    {
        for (int i = 0; i < cases.Length; i++)
        {
            UnionCase @case = cases[i];

            builder.AppendLine()
                   .AppendLine("/// <summary>")
                   .AppendLine($"/// Gets the value when this instance holds a <see cref=\"{Cref(@case)}\"/>.")
                   .AppendLine("/// </summary>")
                   .AppendLine("/// <param name=\"value\">The value when this method returns <see langword=\"true\"/>; otherwise the default value.</param>")
                   .AppendLine($"/// <returns><see langword=\"true\"/> if this instance holds a <see cref=\"{Cref(@case)}\"/>; otherwise <see langword=\"false\"/>.</returns>");

            using (builder.AppendBlock($"public bool TryGetValue(out {@case.TypeName} value)"))
            {
                using (builder.AppendBlock($"if ({TagField} != {Tag(i)})"))
                {
                    builder.AppendLine("value = default;")
                           .AppendLine("return false;");
                }

                builder.AppendLine()
                       .AppendLine($"value = {Storage(@case)};")
                       .AppendLine("return true;");
            }
        }
    }

    private static void EmitValue(SourceBuilder builder, ReadOnlySpan<UnionCase> cases)
    {
        builder.AppendLine()
               .AppendLine("/// <summary>")
               .AppendLine("/// Gets the value this instance holds, boxed, or <see langword=\"null\"/> when it holds none.")
               .AppendLine("/// </summary>");

        using (builder.AppendBlock($"public object? Value => {TagField} switch", "};"))
        {
            for (int i = 0; i < cases.Length; i++)
                builder.AppendLine($"{Tag(i)} => {Storage(cases[i])},");

            builder.AppendLine("_ => null");
        }
    }

    private static bool HasUnmanaged(ReadOnlySpan<UnionCase> cases)
    {
        foreach (UnionCase @case in cases)
        {
            if (!@case.Managed)
                return true;
        }

        return false;
    }

    private static string ManagedField(UnionCase @case)
        => $"_{char.ToLowerInvariant(@case.MemberName[0])}{@case.MemberName[1..]}Case";

    private static string Storage(UnionCase @case)
        => @case.Managed ? ManagedField(@case) : $"{CasesField}.{@case.MemberName}";

    private static string Tag(int index) => (index + 1).ToString(CultureInfo.InvariantCulture);
}
