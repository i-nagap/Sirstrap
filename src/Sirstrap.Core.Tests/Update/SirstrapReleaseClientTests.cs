namespace Sirstrap.Core.Tests.Update
{
    public class SirstrapReleaseClientTests
    {
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
        public async Task GetReleasesAsync_RequestsTheSirstrapReleasesEndpoint()
        {
            StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
            SirstrapReleaseClient releaseClient = new(new HttpClient(handler));

            await releaseClient.GetReleasesAsync();

            Assert.Equal("https://sirstrap.com/releases/", Assert.Single(handler.RequestedUris));
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
