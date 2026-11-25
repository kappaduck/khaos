#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Site;

if (args.Length > 0)
    return Script.Usage("dotnet tools/site/publish-games.cs");

if (!Script.RunsFromRoot(GamePublisher.ComingSoonPath, GamePublisher.ExamplesProject))
    return Script.Failed;

return await Script.RunAsync(static async () => await GamePublisher.PublishAsync() ? 0 : Script.Failed);
