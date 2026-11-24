// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using KappaDuck.Khaos.Tools.Releases;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KappaDuck.Khaos.Tools;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(NuGetIndex))]
[JsonSerializable(typeof(GitHubRelease[]))]
internal partial class JsonContext : JsonSerializerContext;
