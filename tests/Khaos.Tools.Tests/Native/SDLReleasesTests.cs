// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Native;
using System.Net;
using TUnit.Mocks.Http;

namespace KappaDuck.Khaos.Tools.Tests.Native;

internal sealed class SdlReleasesTests : IDisposable
{
    private const string ReleasesRoute = "/repos/libsdl-org/SDL/releases?per_page=100";

    private static readonly SDLLibrary _sdl = new("sdl3", "SDL", "SDL3");

    private readonly MockHttpClient _http = Mock.HttpClient("https://api.github.com");

    public void Dispose() => _http.Dispose();

    [Test]
    public async Task LatestStableAsyncShouldSkipTagsWithoutAVersion()
    {
        _http.Handler.OnGet(ReleasesRoute).RespondWithJson("""
        [
          { "tag_name": "release-next", "draft": false, "prerelease": false },
          { "tag_name": null, "draft": false, "prerelease": false },
          { "draft": false, "prerelease": false }
        ]
        """);

        Version? latest = await SDLReleases.LatestStableAsync(_http, _sdl, null);

        await latest.Should().BeNull();
    }

    [Test]
    public async Task LatestStableAsyncShouldSkipDraftsPreReleasesAndOtherTags()
    {
        _http.Handler.OnGet(ReleasesRoute).RespondWithJson("""
            [
              { "tag_name": "release-3.4.8", "draft": false, "prerelease": false },
              { "tag_name": "release-3.4.10", "draft": false, "prerelease": false },
              { "tag_name": "release-3.6.0", "draft": true, "prerelease": false },
              { "tag_name": "release-3.5.0", "draft": false, "prerelease": true },
              { "tag_name": "preview-3.7.0", "draft": false, "prerelease": false }
            ]
            """);

        Version? latest = await SDLReleases.LatestStableAsync(_http, _sdl, null);

        await latest.Should().BeEqualTo(new Version(3, 4, 10));
    }

    [Test]
    public async Task LatestStableAsyncShouldGiveNullWithoutStableRelease()
    {
        _http.Handler.OnGet(ReleasesRoute).RespondWithJson("""[{ "tag_name": "release-3.5.0", "draft": false, "prerelease": true }]""");

        Version? latest = await SDLReleases.LatestStableAsync(_http, _sdl, null);

        await latest.Should().BeNull();
    }

    [Test]
    public async Task LatestStableAsyncShouldAskForAPageOfOneHundredReleases()
    {
        _http.Handler.OnGet(ReleasesRoute).RespondWithJson("[]");

        await SDLReleases.LatestStableAsync(_http, _sdl, null);

        string? requested = _http.Handler.Requests[0].RequestUri?.AbsoluteUri;
        await requested.Should().BeEqualTo("https://api.github.com/repos/libsdl-org/SDL/releases?per_page=100");
    }

    [Test]
    public async Task LatestStableAsyncShouldSendTheToken()
    {
        _http.Handler.OnRequest(static r => r.Header("Authorization", "Bearer secret"))
                     .RespondWithJson("""[{ "tag_name": "release-3.4.8", "draft": false, "prerelease": false }]""");

        Version? latest = await SDLReleases.LatestStableAsync(_http, _sdl, "secret");

        await latest.Should().BeEqualTo(new Version(3, 4, 8));
    }

    [Test]
    public async Task LatestStableAsyncShouldRefuseAnErrorOfGitHub()
    {
        _http.Handler.OnGet(ReleasesRoute).Respond(HttpStatusCode.Forbidden);

        await Assert.That(async () => await SDLReleases.LatestStableAsync(_http, _sdl, null))
                    .ThrowsExactly<HttpRequestException>();
    }
}
