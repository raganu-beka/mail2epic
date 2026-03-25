namespace Blue.Mail2Epic.Tests.Infrastructure;

public sealed class StubHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => httpClient;
}

