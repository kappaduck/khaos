#!/usr/bin/env dotnet

// Copyright (c) KappaDuck.
// Licensed under the MIT license.

#:project ../Khaos.Tools/Khaos.Tools.csproj

using KappaDuck.Khaos.Tools.Actions;
using KappaDuck.Khaos.Tools.Packages;

IReadOnlyList<string>? platforms = args switch
{
    [] or ["--platform", "all"] => TemplateCheck.Platforms,
    ["--platform", string platform] when TemplateCheck.Platforms.Contains(platform) => [platform],
    _ => null,
};

if (platforms is null)
    return Script.Usage("dotnet tools/packages/check-template.cs [--platform both|desktop|browser|all]");

if (!Script.RunsFromRoot([LocalFeed.PackagesFilter, .. TemplateCheck.Readmes]))
    return Script.Failed;

return await Script.RunAsync(() => TemplateCheck.RunAsync(platforms));
