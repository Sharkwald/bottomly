using System.Net;
using Bottomly.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Bottomly.Tests.Helpers;

internal static class TestHelpers
{
    public const string TestPrefix = "_";

    public static IOptions<BottomlyOptions> CreateOptions(string prefix = TestPrefix) =>
        Options.Create(new BottomlyOptions { Prefix = prefix });

    public static IHttpClientFactory CreateHttpClientFactory(string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(responseContent, statusCode);
        var client = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
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