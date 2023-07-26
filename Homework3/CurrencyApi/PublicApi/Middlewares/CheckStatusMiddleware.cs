using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Extensions;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;

public sealed class CheckStatusMiddleware
{
    private readonly HttpClient      _httpClient;
    private readonly RequestDelegate _next;


    public CheckStatusMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
    {
        _next       = next;
        _httpClient = httpClientFactory.CreateClient(CurrencyApiConstants.DefaultClientName);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        int remaining = await _httpClient.GetRemainingAsync(context.RequestAborted);
        if (remaining <= 0)
        {
            throw new ApiRequestLimitException();
        }

        await _next(context);
    }
}
