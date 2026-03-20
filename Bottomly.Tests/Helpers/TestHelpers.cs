using System.Net;
using Bottomly.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using MsHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace Bottomly.Tests.Helpers;

internal static class TestHelpers
{
    public const string TestPrefix = "_";

    public static IOptions<BottomlyOptions> CreateOptions(string prefix = TestPrefix) =>
        Options.Create(new BottomlyOptions { Prefix = prefix });

    /// <summary>
    ///     Creates a <see cref="System.Net.Http.IHttpClientFactory" /> that returns a fake HTTP response,
    ///     allowing unit tests to exercise HTTP-based commands without hitting real external APIs.
    /// </summary>
    public static MsHttpClientFactory CreateHttpClientFactory(string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(responseContent, statusCode);
        var client = new HttpClient(handler);
        var factory = new Mock<MsHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factory.Object;
    }
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