// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Releases;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KappaDuck.Khaos.Tools;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(NuGetIndex))]
internal partial class JsonContext : JsonSerializerContext;
