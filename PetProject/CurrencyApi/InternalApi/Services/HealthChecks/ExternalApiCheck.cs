using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.HealthChecks;

/// <inheritdoc />
public sealed class ExternalApiCheck : IHealthCheck
{
    private readonly ICurrencyApiService _externalApiService;

    /// <summary>
    /// Встраивание зависимости на внешний API.
    /// </summary>
    /// <param name="externalApiService">Сервис работы с внешним API.</param>
    public ExternalApiCheck(ICurrencyApiService externalApiService)
    {
        _externalApiService = externalApiService;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
                                                          CancellationToken  cancellationToken = new())
    {
        bool isConnected = await _externalApiService.IsConnectedAsync(cancellationToken);

        return isConnected
                   ? HealthCheckResult.Healthy()
                   : HealthCheckResult.Unhealthy("External API isn't responding");
    }
}
