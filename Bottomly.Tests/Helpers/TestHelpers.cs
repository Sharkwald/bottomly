using System.Net;
using Bottomly.Configuration;
using Google.Apis.Http;
using Microsoft.Extensions.Options;
using Moq;
using MsHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace Bottomly.Tests.Helpers;

internal static class TestHelpers
{
    public const string TestPrefix = "_";

    public static IOptions<BottomlyOptions> CreateOptions(string prefix = TestPrefix) =>
        Options.Create(new BottomlyOptions { Prefix = prefix });

    public static MsHttpClientFactory CreateHttpClientFactory(string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(responseContent, statusCode);
        var client = new HttpClient(handler);
        var factory = new Mock<MsHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factory.Object;
    }

    /// <summary>
    /// Creates a <see cref="Google.Apis.Http.IHttpClientFactory"/> that returns a fake HTTP response,
    /// allowing unit tests to exercise <see cref="Bottomly.Commands.GoogleSearchCommand"/> without
    /// hitting the real Google API.
    /// </summary>
    public static Google.Apis.Http.IHttpClientFactory CreateGoogleHttpClientFactory(
        string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new FakeGoogleHttpClientFactory(new FakeHttpMessageHandler(responseContent, statusCode));
}

internal class FakeHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        });
}

internal class FakeGoogleHttpClientFactory(HttpMessageHandler handler) : Google.Apis.Http.IHttpClientFactory
{
    public ConfigurableHttpClient CreateHttpClient(CreateHttpClientArgs args) =>
        new(new ConfigurableMessageHandler(handler));
}