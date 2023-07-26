using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Newtonsoft.Json.Linq;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;

public sealed class CheckStatusMiddleware
{
    private readonly HttpClient      _httpClient;
    private readonly RequestDelegate _next;


    public CheckStatusMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
    {
        _next       = next;
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        int remaining = await this.GetRemainingRequestsAsync(context.RequestAborted);
        if (remaining <= 0)
        {
            throw new ApiRequestLimitException();
        }

        await _next(context);
    }

    private async ValueTask<int> GetRemainingRequestsAsync(CancellationToken stopToken)
    {
        const string        requestUri = "status";
        HttpResponseMessage response   = await _httpClient.GetAsync(requestUri, stopToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new BadHttpRequestException(response.Headers.ToString());
        }

        string  responseBody   = await response.Content.ReadAsStringAsync(stopToken);
        dynamic responseParsed = JObject.Parse(responseBody);
        dynamic quotasSection  = responseParsed.quotas;
        dynamic monthSection   = quotasSection.month;

        return monthSection.remaining;
    }
}
