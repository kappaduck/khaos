// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Hosting.Engine;

internal sealed record AppMetadata
{
    private const string CopyrightKey = "KappaDuck.Khaos.Game.Copyright";
    private const string CreatorKey = "KappaDuck.Khaos.Game.Creator";
    private const string IdentifierKey = "KappaDuck.Khaos.Game.Identifier";
    private const string NameKey = "KappaDuck.Khaos.Game.Name";
    private const string UrlKey = "KappaDuck.Khaos.Game.Url";
    private const string VersionKey = "KappaDuck.Khaos.Game.Version";

    public string? Copyright { get; init; }

    public string? Creator { get; init; }

    public string? Identifier { get; init; }

    public string? Name { get; init; }

    public Uri? Url { get; init; }

    public string? Version { get; init; }

    internal static AppMetadata FromAppContext() => new()
    {
        Copyright = Read(CopyrightKey),
        Creator = Read(CreatorKey),
        Identifier = Read(IdentifierKey),
        Name = Read(NameKey),
        Url = Uri.TryCreate(Read(UrlKey), UriKind.Absolute, out Uri? url) ? url : null,
        Version = Read(VersionKey)
    };

    private static string? Read(string key) => AppContext.GetData(key) is string { Length: > 0 } value ? value : null;
}
