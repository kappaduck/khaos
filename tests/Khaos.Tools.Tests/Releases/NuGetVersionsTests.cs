// Copyright (c) KappaDuck.
// Licensed under the MIT license.

using KappaDuck.Khaos.Tools.Releases;
using System.Net;
using TUnit.Mocks.Http;

namespace KappaDuck.Khaos.Tools.Tests.Releases;

internal sealed class NuGetVersionsTests : IDisposable
{
    private const string NuGet = "https://api.nuget.org";
    private const string IndexPath = "/v3-flatcontainer/kappaduck.khaos/index.json";
    private const string PackageId = "KappaDuck.Khaos";

    private readonly MockHttpClient _http = Mock.HttpClient(NuGet);

    public void Dispose() => _http.Dispose();

    [Test]
    public async Task FetchAsyncShouldAskForTheIndexWithThePackageIdInLowercase()
    {
        _http.Handler.OnGet(IndexPath).RespondWithJson("""{ "versions": [] }""");

        await NuGetVersions.FetchAsync(_http, PackageId);

        string? requested = _http.Handler.Requests[0].RequestUri?.AbsoluteUri;
        await requested.Should().BeEqualTo(NuGet + IndexPath);
    }

    [Test]
    public async Task FetchAsyncShouldReadTheVersionsOfTheIndex()
    {
        _http.Handler.OnGet(IndexPath).RespondWithJson("""{ "versions": ["0.1.0-beta.1", "0.1.0-beta.2"] }""");

        IReadOnlyList<string> versions = (await NuGetVersions.FetchAsync(_http, PackageId))!;

        await versions[0].Should().BeEqualTo("0.1.0-beta.1");
        await versions[1].Should().BeEqualTo("0.1.0-beta.2");
    }

    [Test]
    public async Task FetchAsyncShouldGiveNullForAPackageNuGetDoesNotHave()
    {
        _http.Handler.OnGet(IndexPath).Respond(HttpStatusCode.NotFound);

        IReadOnlyList<string>? versions = await NuGetVersions.FetchAsync(_http, PackageId);

        await versions.Should().BeNull();
    }

    [Test]
    public async Task FetchAsyncShouldRefuseAnyOtherAnswer()
    {
        _http.Handler.OnGet(IndexPath).Respond(HttpStatusCode.InternalServerError);

        await Assert.That(async () => await NuGetVersions.FetchAsync(_http, PackageId))
                    .ThrowsExactly<HttpRequestException>();
    }
}
