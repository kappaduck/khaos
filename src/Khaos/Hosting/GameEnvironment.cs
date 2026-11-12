// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace KappaDuck.Khaos.Hosting;

internal sealed class GameEnvironment : IHostEnvironment
{
    internal const string BuildKey = "KappaDuck.Khaos.Environment";
    internal const string DotnetPrefix = "DOTNET_";
    internal const string EmptyName = "The environment name is empty. Name one, such as --environment Staging, or leave it out for Production.";
    internal const string Key = "environment";
    internal const string KhaosPrefix = "KHAOS_";

    private GameEnvironment(string name, string contentRoot)
    {
        ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
        ContentRootFileProvider = new PhysicalFileProvider(contentRoot);
        ContentRootPath = contentRoot;
        EnvironmentName = name;
    }

    public string ApplicationName { get; set; }

    public IFileProvider ContentRootFileProvider { get; set; }

    public string ContentRootPath { get; set; }

    public string EnvironmentName { get; set; }

    internal static GameEnvironment Resolve(string[] args, string contentRoot)
    {
        IConfigurationRoot named = new ConfigurationBuilder()
            .AddEnvironmentVariables(DotnetPrefix)
            .AddEnvironmentVariables(KhaosPrefix)
            .AddCommandLine(args)
            .Build();

        string? name = named[Key];

        if (name is not null && string.IsNullOrWhiteSpace(name))
            throw new KhaosException(EmptyName);

        return new GameEnvironment(name ?? BuiltName(), contentRoot);
    }

    private static string BuiltName() => AppContext.GetData(BuildKey) as string ?? Environments.Production;
}
