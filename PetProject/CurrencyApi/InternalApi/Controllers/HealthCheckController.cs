using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers;

/// <summary>
///     Методы для проверки работоспособности InternalApi
/// </summary>
[Route("healthcheck")]
public class HealthCheckController : ControllerBase
{
    private readonly ICurrencyApiService _externalApiService;

    /// <summary>
    /// Проверка связи с API.
    /// </summary>
    /// <param name="externalApiService">Сервис сторонннего API.</param>
    public HealthCheckController(ICurrencyApiService externalApiService)
    {
        _externalApiService = externalApiService;
    }

    /// <summary>
    ///     Проверить что API работает
    /// </summary>
    /// <param name="checkExternalApi">
    ///     Необходимо проверить работоспособность внешнего API. Если FALSE или NULL - проверяется
    ///     работоспособность только текущего API
    /// </param>
    /// <param name="stopToken">Токен отмены выполнения.</param>
    /// <response code="200">
    ///     Возвращает если удалось получить доступ к API
    /// </response>
    /// <response code="400">
    ///     Возвращает если удалось не удалось получить доступ к API
    /// </response>
    [HttpGet]
    public async Task<HealthResult> CheckAsync(bool? checkExternalApi, CancellationToken stopToken)
    {
        if (checkExternalApi is not true)
        {
            return new HealthResult
                   {
                       Status    = HealthResult.CheckStatus.Ok,
                       CheckedOn = DateTimeOffset.Now
                   };
        }

        bool isConnected = await _externalApiService.IsConnectedAsync(stopToken);
        HealthResult.CheckStatus status = isConnected
                                              ? HealthResult.CheckStatus.Ok
                                              : HealthResult.CheckStatus.Failed;

        return new HealthResult
               {
                   Status    = status,
                   CheckedOn = DateTimeOffset.Now
               };
    }
}
