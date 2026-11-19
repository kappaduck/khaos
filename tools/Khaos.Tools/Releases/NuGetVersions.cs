// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Net;

namespace KappaDuck.Khaos.Tools.Releases;

public static class NuGetVersions
{
    public static async Task<IReadOnlyList<string>?> FetchAsync(HttpClient http, string packageId)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(packageId);

        Uri uri = IndexOf(packageId);
        Console.WriteLine($"Querying NuGet: {uri}");

        using HttpResponseMessage response = await http.GetAsync(uri);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.StatusCode != HttpStatusCode.OK)
            throw new HttpRequestException($"Unexpected HTTP {(int)response.StatusCode} from NuGet.");

        NuGetIndex? index = await response.Content.ReadFromJsonAsync(JsonContext.Default.NuGetIndex);
        return index?.Versions ?? [];
    }

    private static Uri IndexOf(string packageId)
        => new($"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json");
}
