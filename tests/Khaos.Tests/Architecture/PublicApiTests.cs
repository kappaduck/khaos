// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace KappaDuck.Khaos.Tests.Architecture;

internal sealed class PublicApiTests
{
    private const string InteropNamespace = "KappaDuck.Khaos.Interop";
    private const string Justification = "The test host is never trimmed or published ahead of time, so every type and member is present. The suite still analyses the framework itself for trimming.";

    // TODO: Loaded by name until Khaos has a public type to anchor on; swap for typeof(...).Assembly then.
    private static readonly Assembly _khaos = System.Reflection.Assembly.Load("KappaDuck.Khaos");

    [Test]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = Justification)]
    public async Task InteropTypesShouldStayInternal()
    {
        IEnumerable<string> leaked = _khaos.GetTypes()
                                         .Where(static type => IsInterop(type) && type.IsVisible)
                                         .Select(static type => type.FullName!)
                                         .Order();

        await Report(leaked).Should().BeEqualTo(string.Empty);
    }

    [Test]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = Justification)]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern", Justification = Justification)]
    public async Task PublicApiShouldNotExposeInteropTypes()
    {
        IEnumerable<string> leaked = from type in _khaos.GetTypes()
                                     where type.IsVisible
                                     from member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                                     from used in Signature(member).SelectMany(Unwrap)
                                     where IsInterop(used)
                                     select $"{type.FullName}.{member.Name} exposes {used.FullName}";

        await Report(leaked.Distinct().Order()).Should().BeEqualTo(string.Empty);
    }

    private static bool IsInterop(Type type)
        => type.Namespace?.StartsWith(InteropNamespace, StringComparison.Ordinal) == true;

    private static IEnumerable<Type> Signature(MemberInfo member) => member switch
    {
        MethodInfo method => method.GetParameters().Select(static parameter => parameter.ParameterType).Append(method.ReturnType),
        ConstructorInfo constructor => constructor.GetParameters().Select(static parameter => parameter.ParameterType),
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        EventInfo handler when handler.EventHandlerType is not null => [handler.EventHandlerType],
        _ => []
    };

    private static IEnumerable<Type> Unwrap(Type type)
    {
        while (type.HasElementType)
            type = type.GetElementType()!;

        yield return type;

        foreach (Type argument in type.GetGenericArguments())
        {
            foreach (Type unwrapped in Unwrap(argument))
                yield return unwrapped;
        }
    }

    private static string Report(IEnumerable<string> leaked) => string.Join(Environment.NewLine, leaked);
}
