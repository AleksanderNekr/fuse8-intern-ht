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
        var apiKey = _configuration.GetValue<string>("API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            request.Headers.Add("apikey", apiKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
