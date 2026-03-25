using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Blue.Mail2Epic.Tests.Infrastructure;

public sealed class JiraStubHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

        if (request.Method == HttpMethod.Get && path.Contains("/rest/api/2/issue/", StringComparison.OrdinalIgnoreCase))
        {
            var issueKey = path.Split('/').Last();
            var payload = new
            {
                key = issueKey,
                fields = new
                {
                    summary = "Stub summary",
                    description = "Stub description for testing.",
                    comment = new
                    {
                        comments = new[]
                        {
                            new { body = "Stub comment." }
                        }
                    }
                }
            };

            return Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, payload));
        }

        if (request.Method == HttpMethod.Post && path.Contains("/rest/api/2/issue/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, object payload)
    {
        var json = JsonConvert.SerializeObject(payload);
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }
}

