// Copyright (c) KappaDuck.
// Licensed under the MIT license.

namespace KappaDuck.Khaos.Tests.Helpers;

internal static class GameMetadata
{
    internal const string Copyright = "KappaDuck.Khaos.Game.Copyright";
    internal const string Creator = "KappaDuck.Khaos.Game.Creator";
    internal const string Identifier = "KappaDuck.Khaos.Game.Identifier";
    internal const string Name = "KappaDuck.Khaos.Game.Name";
    internal const string Url = "KappaDuck.Khaos.Game.Url";
    internal const string Version = "KappaDuck.Khaos.Game.Version";

    internal static void Clear()
    {
        AppContext.SetData(Copyright, null);
        AppContext.SetData(Creator, null);
        AppContext.SetData(Identifier, null);
        AppContext.SetData(Name, null);
        AppContext.SetData(Url, null);
        AppContext.SetData(Version, null);
    }
}
