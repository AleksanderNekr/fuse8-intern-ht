using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.AspNetCore.Mvc;
using static Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers.HealthCheckResult;

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
    public async Task<HealthCheckResult> Check(bool? checkExternalApi, CancellationToken stopToken)
    {
        if (checkExternalApi is not true)
        {
            return new HealthCheckResult { Status = CheckStatus.Ok, CheckedOn = DateTimeOffset.Now };
        }

        bool isConnected = await _externalApiService.IsConnectedAsync(stopToken);

        return isConnected
                   ? new HealthCheckResult { Status = CheckStatus.Ok, CheckedOn     = DateTimeOffset.Now }
                   : new HealthCheckResult { Status = CheckStatus.Failed, CheckedOn = DateTimeOffset.Now };
    }
}

/// <summary>
///     Результат проверки работоспособности API
/// </summary>
public record HealthCheckResult
{
    /// <summary>
    ///     Статус API
    /// </summary>
    public enum CheckStatus
    {
        /// <summary>
        ///     API работает
        /// </summary>
        Ok = 1,

        /// <summary>
        ///     Ошибка в работе API
        /// </summary>
        Failed = 2,
    }

    /// <summary>
    ///     Дата проверки
    /// </summary>
    public DateTimeOffset CheckedOn { get; init; }

    /// <summary>
    ///     Статус работоспособности API
    /// </summary>
    public CheckStatus Status { get; init; }
}
