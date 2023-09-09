using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.AspNetCore.Mvc;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers;

/// <summary>
///     Контроллер для работы с курсами валют.
/// </summary>
[Route("currency")]
[ApiController]
public class CurrencyApiController : ControllerBase
{
    private readonly ICurrencyApiService _service;
    private readonly CurrenciesSettings  _settings;

    /// <summary>
    ///     Инициализация контроллера курсов валют.
    /// </summary>
    /// <param name="service"><see cref="ICurrencyApiService" /> сервис получения информации от CurrencyApi.</param>
    /// <param name="settings">Настройки текущего API.</param>
    public CurrencyApiController(ICurrencyApiService service, CurrenciesSettings settings)
    {
        _service  = service;
        _settings = settings;
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
    public async Task<ActionResult<CurrencyInfo>> GetCurrency(CurrencyType currencyCode, CancellationToken stopToken)
    {
        return await _service.GetCurrencyInfoAsync(currencyCode,
                                                   _settings.BaseCurrency,
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
    public async Task<ActionResult<CurrencyOnDateInfo>> GetCurrency(CurrencyType      currencyCode,
                                                                    DateOnly          date,
                                                                    CancellationToken stopToken)
    {
        return await _service.GetCurrencyInfoOnDateAsync(currencyCode,
                                                         _settings.BaseCurrency,
                                                         date,
                                                         stopToken);
    }


    /// <summary>
    ///     Получение информации об аккаунте на месяц.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось получить информацию.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить информацию.
    /// </response>
    /// <returns>
    ///     <see cref="MonthSection"/> объект.
    ///     <example>
    ///         <code>
    ///     {
    ///         "total": 300,
    ///         "used": 10,
    ///         "remaining": 290
    ///     }
    ///     </code>
    ///     </example>
    /// </returns>
    [HttpGet("settings")]
    public async Task<ActionResult<MonthSection>> GetSettingsInfo(CancellationToken stopToken)
    {
        MonthSection monthSection = await _service.GetMonthSectionAsync(stopToken);

        return monthSection;
    }

    /// <summary>
    /// Проверка соединения с внешним API.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>true – удалось подключиться к API, false – нет.</returns>
    [HttpGet("is_connected")]
    public async Task<ActionResult<bool>> IsConnected(CancellationToken stopToken)
    {
        bool isConnected = await _service.IsConnectedAsync(stopToken);

        return isConnected;
    }

    /// <summary>
    /// Получение текущего курса валют относительно базовой.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>Массив информаций о валютах.</returns>
    [HttpGet("all_currencies")]
    public async Task<ActionResult<CurrencyInfo[]>> GetAllCurrencies(CancellationToken stopToken)
    {
        CurrencyInfo[] currencyInfos = await _service.GetAllCurrentCurrenciesAsync(_settings.BaseCurrency, stopToken);

        return currencyInfos;
    }

    /// <summary>
    /// Получение курса валют относительно базовой на определенную дату.
    /// </summary>
    /// <param name="date">Дата, на которую получена информация.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>Массив информаций о валютах.</returns>
    [HttpGet("all_currencies/{date}")]
    public async Task<ActionResult<CurrenciesOnDate>> GetAllCurrenciesOnDate(DateOnly date, CancellationToken stopToken)
    {
        CurrenciesOnDate currenciesOnDate =
            await _service.GetAllCurrenciesOnDateAsync(_settings.BaseCurrency, date, stopToken);

        return currenciesOnDate;
    }
}
