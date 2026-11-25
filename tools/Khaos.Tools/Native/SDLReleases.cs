// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace KappaDuck.Khaos.Tools.Native;

public static class SDLReleases
{
    private const string TagPrefix = "release-";

    private static readonly Uri _gitHub = new("https://api.github.com/");

    public static async Task<Version?> LatestStableAsync(HttpClient http, SDLLibrary library, string? token)
    {
        ArgumentNullException.ThrowIfNull(http);
        ArgumentNullException.ThrowIfNull(library);

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri(_gitHub, $"repos/libsdl-org/{library.Repository}/releases?per_page=100"));

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("khaos-sdl-updater", "1.0"));
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            string reason = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"GitHub returned {(int)response.StatusCode} for libsdl-org/{library.Repository}: {reason}");
        }

        GitHubRelease[]? releases = await response.Content.ReadFromJsonAsync(JsonContext.Default.GitHubReleaseArray);
        Version? latest = null;

        foreach (GitHubRelease release in releases ?? [])
        {
            if (release.Draft || release.Prerelease || !TryParseTag(release.Tag, out Version? version))
                continue;

            if (latest is null || version > latest)
                latest = version;
        }

        return latest;
    }

    private static bool TryParseTag([NotNullWhen(true)] string? tag, [NotNullWhen(true)] out Version? version)
    {
        version = null;

        if (tag?.StartsWith(TagPrefix, StringComparison.Ordinal) != true)
            return false;

        return Version.TryParse(tag[TagPrefix.Length..], out version);
    }
}
