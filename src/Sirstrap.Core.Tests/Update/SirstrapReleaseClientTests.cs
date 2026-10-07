namespace Sirstrap.Core.Tests.Update
{
    public class SirstrapReleaseClientTests
    {
        private static bool IsAruba(HttpRequestMessage request) => request.RequestUri!.ToString() == SirstrapReleaseClient.ARUBA_RELEASES_URI;

        [Fact]
        public async Task GetReleasesAsync_ParsesReleaseArray()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, """[{"tag_name":"v1.0.0.0-beta","draft":false,"body":"a"},{"tag_name":"v2.0.0.0-beta","draft":true,"body":"b"}]""");
            SirstrapReleaseClient releaseClient = new(client);

            var releases = await releaseClient.GetReleasesAsync();

            Assert.Equal(2, releases.Count);
            Assert.Equal("v1.0.0.0-beta", releases[0].TagName);
            Assert.True(releases[1].IsDraft);
        }

        [Fact]
        public async Task GetReleasesAsync_UsesAruba_WithoutCallingGitHub()
        {
            StubHttpMessageHandler handler = new(request => IsAruba(request)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""[{"tag_name":"v4.0.0.0-beta","draft":false,"body":"d"}]""") }
                : throw new InvalidOperationException("GitHub must not be called"));
            SirstrapReleaseClient releaseClient = new(new HttpClient(handler));

            var releases = await releaseClient.GetReleasesAsync();

            Assert.Equal("v4.0.0.0-beta", Assert.Single(releases).TagName);
            Assert.Equal(1, handler.CallCount);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, "")]
        [InlineData(HttpStatusCode.OK, "[]")]
        [InlineData(HttpStatusCode.OK, "not-json")]
        public async Task GetReleasesAsync_FallsBackToGitHubAccounts_WhenArubaFails(HttpStatusCode arubaStatus, string arubaBody)
        {
            StubHttpMessageHandler handler = new(request =>
            {
                if (IsAruba(request))
                    return new HttpResponseMessage(arubaStatus) { Content = new StringContent(arubaBody) };

                return request.RequestUri!.ToString().Contains(GitHubAccounts.Primary, StringComparison.OrdinalIgnoreCase)
                    ? new HttpResponseMessage(HttpStatusCode.NotFound)
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""[{"tag_name":"v3.0.0.0-beta","draft":false,"body":"c"}]""") };
            });
            SirstrapReleaseClient releaseClient = new(new HttpClient(handler));

            var releases = await releaseClient.GetReleasesAsync();

            Assert.Equal("v3.0.0.0-beta", Assert.Single(releases).TagName);
            Assert.Equal(1 + GitHubAccounts.All.Count, handler.CallCount);
        }

        [Fact]
        public async Task GetReleasesAsync_ReturnsEmpty_OnException()
        {
            HttpClient client = StubHttpMessageHandler.Client(_ => throw new HttpRequestException("down"));
            SirstrapReleaseClient releaseClient = new(client);

            Assert.Empty(await releaseClient.GetReleasesAsync());
        }

        [Fact]
        public async Task GetReleasesAsync_ReturnsEmpty_OnMalformedJson()
        {
            HttpClient client = StubHttpMessageHandler.Client(HttpStatusCode.OK, "not-json");
            SirstrapReleaseClient releaseClient = new(client);

            Assert.Empty(await releaseClient.GetReleasesAsync());
        }
    }
}
