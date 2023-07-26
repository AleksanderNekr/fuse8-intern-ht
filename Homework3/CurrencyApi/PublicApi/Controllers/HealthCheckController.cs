using Microsoft.AspNetCore.Mvc;
using static Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers.HealthCheckResult;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;

/// <summary>
/// Методы для проверки работоспособности PublicApi
/// </summary>
[Route("healthcheck")]
public class HealthCheckController : ControllerBase
{
    private readonly HttpClient _externalHttpClient;

    public HealthCheckController(IHttpClientFactory clientFactory)
    {
        this._externalHttpClient = clientFactory.CreateClient("DefaultClient");
    }

    /// <summary>
    /// Проверить что API работает
    /// </summary>
    /// <param name="checkExternalApi">Необходимо проверить работоспособность внешнего API. Если FALSE или NULL - проверяется работоспособность только текущего API</param>
    /// <param name="stopToken">Токен отмены выполнения.</param>
    /// <response code="200">
    /// Возвращает если удалось получить доступ к API
    /// </response>
    /// <response code="400">
    /// Возвращает если удалось не удалось получить доступ к API
    /// </response>
    [HttpGet]
    public async Task<HealthCheckResult> Check(bool? checkExternalApi, CancellationToken stopToken)
    {
        if (checkExternalApi is not true)
        {
            return new HealthCheckResult { Status = CheckStatus.Ok, CheckedOn = DateTimeOffset.Now };
        }

        const string        requestUri = "status";
        HttpResponseMessage response   = await this._externalHttpClient.GetAsync(requestUri, stopToken);

        return response.IsSuccessStatusCode
                   ? new HealthCheckResult { Status = CheckStatus.Ok, CheckedOn     = DateTimeOffset.Now }
                   : new HealthCheckResult { Status = CheckStatus.Failed, CheckedOn = DateTimeOffset.Now };

    }
}

/// <summary>
/// Результат проверки работоспособности API
/// </summary>
public record HealthCheckResult
{
    /// <summary>
    /// Дата проверки
    /// </summary>
    public DateTimeOffset CheckedOn { get; init; }

    /// <summary>
    /// Статус работоспособности API
    /// </summary>
    public CheckStatus Status { get; init; }

    /// <summary>
    /// Статус API
    /// </summary>
    public enum CheckStatus
    {
        /// <summary>
        /// API работает
        /// </summary>
        Ok = 1,

        /// <summary>
        /// Ошибка в работе API
        /// </summary>
        Failed = 2,
    }
}
