// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KappaDuck.Khaos.Tests.Helpers;

internal sealed class TestHost
{
    internal const string BuildEnvironment = "KappaDuck.Khaos.Environment";
    internal const string DotnetEnvironment = "DOTNET_ENVIRONMENT";
    internal const string KhaosEnvironment = "KHAOS_ENVIRONMENT";

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("khaos-settings-");

    internal GameProbe Probe { get; } = new();

    internal KhaosApplicationBuilder Create(params string[] args)
    {
        KhaosApplicationBuilder builder = new(args, _root.FullName);
        builder.Services.AddSingleton(Probe);

        return builder;
    }

    internal void Delete()
    {
        ForgetEnvironment();
        _root.Delete(true);
    }

    internal void Write(string name, string json) => File.WriteAllText(Path.Combine(_root.FullName, name), json);

    internal static void ForgetEnvironment()
    {
        Environment.SetEnvironmentVariable(DotnetEnvironment, null);
        Environment.SetEnvironmentVariable(KhaosEnvironment, null);
        AppContext.SetData(BuildEnvironment, null);
    }
}
