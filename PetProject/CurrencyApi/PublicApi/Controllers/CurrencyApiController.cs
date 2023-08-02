using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;

/// <summary>
///     Контроллер для работы с курсами валют.
/// </summary>
[Route("currency")]
[ApiController]
public class CurrencyApiController : ControllerBase
{
    private readonly ICurrencyApiService            _service;
    private readonly CurrenciesSettings             _settings;
    private readonly ILogger<CurrencyApiController> _logger;

    /// <summary>
    ///     Инициализация контроллера курсов валют.
    /// </summary>
    /// <param name="service"><see cref="ICurrencyApiService" /> сервис получения информации от CurrencyApi.</param>
    /// <param name="optionsMonitor">Настройки текущего API.</param>
    public CurrencyApiController(ICurrencyApiService                 service,
                                 IOptionsMonitor<CurrenciesSettings> optionsMonitor, ILogger<CurrencyApiController> logger)
    {
        _service     = service;
        _logger = logger;
        _settings    = optionsMonitor.CurrentValue;
    }

    /// <summary>
    ///     Получение курса валюты по умолчанию.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось получить курс валюты по умолчанию.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить курс валюты по умолчанию.
    /// </response>
    /// <returns>
    ///     JSON
    ///     <example>
    ///         <code>
    ///     {
    ///         "code": "RUB",
    ///         "value": 90.50
    ///     }
    ///     </code>
    ///     </example>
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<CurrencyInfo>> GetDefaultCurrency(CancellationToken stopToken)
    {
        _logger.LogError("TEST ERROR LOG");

        return await _service.GetCurrencyInfoAsync(_settings.DefaultCurrency,
                                                   _settings.BaseCurrency,
                                                   _settings.DecimalPlace,
                                                   stopToken);
    }

    /// <summary>
    ///     Получение курса валюты.
    /// </summary>
    /// <param name="currencyCode">Код валюты.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось получить курс валюты.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить курс валюты.
    /// </response>
    /// <response code="404">
    ///     Возвращает, если валюта не найдена.
    /// </response>
    /// <returns>
    ///     JSON
    ///     <example>
    ///         <code>
    ///     {
    ///         "code": "RUB",
    ///         "value": 90.50
    ///     }
    ///     </code>
    ///     </example>
    /// </returns>
    [HttpGet("{currencyCode}")]
    public async Task<ActionResult<CurrencyInfo>> GetCurrency(string currencyCode, CancellationToken stopToken)
    {
        return await _service.GetCurrencyInfoAsync(currencyCode,
                                                   _settings.BaseCurrency,
                                                   _settings.DecimalPlace,
                                                   stopToken);
    }


    /// <summary>
    ///     Получение курса валюты на определенную дату.
    /// </summary>
    /// <param name="currencyCode">Код валюты.</param>
    /// <param name="date">Дата.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось получить курс валюты.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить курс валюты.
    /// </response>
    /// <response code="404">
    ///     Возвращает, если валюта не найдена.
    /// </response>
    /// <returns>
    ///     JSON
    ///     <example>
    ///         <code>
    ///     {
    ///         "date": "2020-12-25",
    ///         "code": "RUB",
    ///         "value": 90.50
    ///     }
    ///     </code>
    ///     </example>
    /// </returns>
    [HttpGet("{currencyCode}/{date}")]
    public async Task<ActionResult<CurrencyOnDateInfo>> GetCurrency(string            currencyCode,
                                                                    DateOnly          date,
                                                                    CancellationToken stopToken)
    {
        return await _service.GetCurrencyInfoOnDateAsync(currencyCode,
                                                         _settings.BaseCurrency,
                                                         _settings.DecimalPlace,
                                                         date,
                                                         stopToken);
    }

    /// <summary>
    ///     Получение текущих настроек приложения.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось получить настройки.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить настройки.
    /// </response>
    /// <returns>
    ///     JSON
    ///     <example>
    ///         <code>
    ///     {
    ///         "defaultCurrency": "RUB",
    ///         "baseCurrency": "USD",
    ///         "requestLimit": 300,
    ///         "requestCount": 0,
    ///         "currencyRoundCount": 2
    ///     }
    ///     </code>
    ///     </example>
    /// </returns>
    [HttpGet("settings")]
    public async Task<ActionResult<SettingsInfo>> GetSettingsInfo(CancellationToken stopToken)
    {
        MonthSection monthSection = await _service.GetMonthSectionAsync(stopToken);

        return new SettingsInfo
               {
                   DefaultCurrency    = _settings.DefaultCurrency,
                   BaseCurrency       = _settings.BaseCurrency,
                   RequestLimit       = monthSection.Total,
                   RequestCount       = monthSection.Used,
                   CurrencyRoundCount = _settings.DecimalPlace,
               };
    }
}
