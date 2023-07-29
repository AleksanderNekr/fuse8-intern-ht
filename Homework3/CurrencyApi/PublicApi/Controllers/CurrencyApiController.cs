using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Extensions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
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
    private readonly string     _baseCurrency;
    private readonly int        _decimalPlace;
    private readonly string     _defaultCurrency;
    private readonly HttpClient _httpClient;

    /// <summary>
    ///     Инициализация контроллера курсов валют.
    /// </summary>
    /// <param name="clientFactory">Фабрика создания HTTP-клиента.</param>
    /// <param name="currenciesSettings">Настройки получения информации о валютах.</param>
    public CurrencyApiController(IHttpClientFactory                  clientFactory,
                                 IOptionsMonitor<CurrenciesSettings> currenciesSettings)
    {
        _httpClient = clientFactory.CreateClient(CurrencyApiConstants.DefaultClientName);

        _baseCurrency    = currenciesSettings.CurrentValue.BaseCurrency;
        _defaultCurrency = currenciesSettings.CurrentValue.DefaultCurrency;
        _decimalPlace    = currenciesSettings.CurrentValue.DecimalPlace;
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
    /// <example>
    ///     <code>
    ///     {
    ///         "code": "RUB",
    ///         "value": 90.50
    ///     }
    ///     </code>
    /// </example>
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetDefaultCurrency(CancellationToken stopToken)
    {
        return await GetCurrencyInfo(_defaultCurrency, _baseCurrency, stopToken);
    }

    /// <summary>
    /// Получение курса валюты.
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
    /// JSON
    /// <example>
    ///     <code>
    ///     {
    ///         "code": "RUB",
    ///         "value": 90.50
    ///     }
    ///     </code>
    /// </example>
    /// </returns>
    [HttpGet("{currencyCode}")]
    public async Task<IActionResult> GetCurrency(string currencyCode, CancellationToken stopToken)
    {
        return await GetCurrencyInfo(currencyCode, _baseCurrency, stopToken);
    }


    /// <summary>
    /// Получение курса валюты на определенную дату.
    /// </summary>
    /// <param name="currencyCode">Код валюты.</param>
    /// <param name="date">Дата в формате yyyy-MM-dd.</param>
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
    /// JSON
    /// <example>
    ///     <code>
    ///     {
    ///         "date": "2020-12-25",
    ///         "code": "RUB",
    ///         "value": 90.50
    ///     }
    ///     </code>
    /// </example>
    /// </returns>
    [HttpGet("{currencyCode}/{date}")]
    public async Task<IActionResult> GetCurrency(string currencyCode, DateOnly date, CancellationToken stopToken)
    {
        var dateFormatted = date.ToString("yyyy-MM-dd");

        return await GetCurrencyInfo(currencyCode, _baseCurrency, stopToken, dateFormatted);
    }

    /// <summary>
    /// Получение текущих настроек приложения.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось получить настройки.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить настройки.
    /// </response>
    /// <returns>
    /// JSON
    /// <example>
    ///     <code>
    ///     {
    ///         "defaultCurrency": "RUB",
    ///         "baseCurrency": "USD",
    ///         "requestLimit": 300,
    ///         "requestCount": 0,
    ///         "currencyRoundCount": 2
    ///     }
    ///     </code>
    /// </example>
    /// </returns>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsInfo(CancellationToken stopToken)
    {
        MonthSection monthSection = await CurrencyApiExtensions.GetMonthSectionAsync(_httpClient, stopToken);

        return new JsonResult(new
                              {
                                  defaultCurrency    = _defaultCurrency,
                                  baseCurrency       = _baseCurrency,
                                  requestLimit       = monthSection.Total,
                                  requestCount       = monthSection.Used,
                                  currencyRoundCount = _decimalPlace
                              });
    }

    private async Task<IActionResult> GetCurrencyInfo(string            defaultCurrency,
                                                      string            baseCurrency,
                                                      CancellationToken stopToken,
                                                      string?           date = null)
    {

        decimal value = await _httpClient.GetCurrencyValue(defaultCurrency, baseCurrency, date, stopToken);

        return ReturnJson(Math.Round(value, _decimalPlace));

        IActionResult ReturnJson(decimal roundedValue)
        {
            if (date is null)
            {
                return new JsonResult(new
                                      {
                                          code  = defaultCurrency,
                                          value = roundedValue,
                                      });
            }

            return new JsonResult(new
                                  {
                                      date,
                                      code  = defaultCurrency,
                                      value = roundedValue,
                                  });
        }
    }
}
