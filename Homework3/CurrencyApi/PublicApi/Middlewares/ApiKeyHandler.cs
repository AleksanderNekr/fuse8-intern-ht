using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;

internal class ApiKeyHandler : DelegatingHandler
{
    private readonly IConfiguration _configuration;

    public ApiKeyHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                           CancellationToken  cancellationToken)
    {
        var apiKey = _configuration.GetValue<string>(CurrencyApiConstants.ApiSettingsKey);
        if (!string.IsNullOrEmpty(apiKey))
        {
            request.Headers.Add(CurrencyApiConstants.ApiKeyHeader, apiKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
