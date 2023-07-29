using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Extensions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

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
        MonthSection monthSection = await CurrencyApiExtensions.GetMonthSectionAsync(_httpClient, context.RequestAborted);
        if (monthSection.Remaining <= 0)
        {
            throw new ApiRequestLimitException();
        }

        await _next(context);
    }
}
