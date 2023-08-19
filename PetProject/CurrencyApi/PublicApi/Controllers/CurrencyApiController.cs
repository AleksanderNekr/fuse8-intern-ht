using System.ComponentModel.DataAnnotations;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;

/// <summary>
///     Контроллер для работы с курсами валют.
/// </summary>
[Route("currency")]
[ApiController]
public class CurrencyApiController : ControllerBase
{
    private readonly ICurrencyApiService   _service;
    private readonly CurrencyPublicContext _context;

    private const int ExpectedSettingsRowsChanged = 1;

    /// <summary>
    ///     Инициализация контроллера курсов валют.
    /// </summary>
    /// <param name="service"><see cref="ICurrencyApiService" /> сервис получения информации от CurrencyApi.</param>
    /// <param name="context">Контекст базы данных.</param>
    public CurrencyApiController(ICurrencyApiService   service,
                                 CurrencyPublicContext context)
    {
        _service = service;
        _context = context;
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
        CurrenciesSettings settings = await _context.Settings.SingleAsync(cancellationToken: stopToken);

        return await _service.GetCurrencyInfoAsync(Enum.Parse<CurrencyType>(settings.DefaultCurrency, ignoreCase: true),
                                                   settings.DecimalPlace,
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
    public async Task<ActionResult<CurrencyInfo>> GetCurrency(CurrencyType currencyCode, CancellationToken stopToken)
    {
        CurrenciesSettings settings = await _context.Settings.SingleAsync(cancellationToken: stopToken);

        return await _service.GetCurrencyInfoAsync(currencyCode, settings.DecimalPlace, stopToken);
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
        CurrenciesSettings settings = await _context.Settings.SingleAsync(cancellationToken: stopToken);

        return await _service.GetCurrencyInfoOnDateAsync(currencyCode, settings.DecimalPlace, date, stopToken);
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
    ///     <code>
    ///     {
    ///       "defaultCurrency": "RUB",
    ///       "baseCurrency": "USD",
    ///       "newRequestsAvailable": bool,
    ///       "currencyRoundCount": 2
    ///     }
    ///     </code>
    ///     </example>
    /// </returns>
    [HttpGet("settings")]
    public async Task<ActionResult<SettingsInfo>> GetSettingsInfo(CancellationToken stopToken)
    {
        SettingsInfo settings = await _service.GetSettingsAsync(stopToken);

        return settings;
    }

    /// <summary>
    /// Изменение валюты по умолчанию.
    /// </summary>
    /// <param name="newCurrency">Новая валюта.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось обновить валюту.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось обновить валюту.
    /// </response>
    /// <returns>Результат действия: <see cref="OkResult"/> – валюта обновлена, <see cref="BadRequestResult"/> – не удалось изменить валюту.</returns>
    [HttpPut("settings/change_default_currency")]
    public async Task<IActionResult> UpdateDefaultCurrency(CurrencyType newCurrency, CancellationToken stopToken)
    {
        int rowsChanged = await _context.Settings.ExecuteUpdateAsync(calls => calls.SetProperty(
                                                                          static settings => settings.DefaultCurrency,
                                                                          newCurrency.ToString()),
                                                                     stopToken);

        bool updated = rowsChanged == ExpectedSettingsRowsChanged;

        return updated
                   ? Ok()
                   : BadRequest("Can't change default currency");
    }

    /// <summary>
    /// Изменение количества знаков после запятой для округления.
    /// </summary>
    /// <param name="newDecimalPlace">Новое количество знаков после запятой – поддерживаются ограниченные значения.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <response code="200">
    ///     Возвращает, если удалось обновить количество знаков.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось обновить количество знаков.
    /// </response>
    /// <returns>Результат действия: <see cref="OkResult"/> – количество знаков обновлено, <see cref="BadRequestResult"/> – не удалось изменить количество знаков.</returns>
    [HttpPut("settings/change_currency_round_count")]
    public async Task<IActionResult> UpdateDecimalPlace(
        [Range(CurrenciesSettings.MinimumDecimalPlace, CurrenciesSettings.MaximumDecimalPlace)]
        int newDecimalPlace,
        CancellationToken stopToken)
    {
        int rowsChanged = await _context.Settings.ExecuteUpdateAsync(calls => calls.SetProperty(
                                                                          static settings => settings.DecimalPlace,
                                                                          newDecimalPlace),
                                                                     stopToken);

        bool updated = rowsChanged == ExpectedSettingsRowsChanged;

        return updated
                   ? Ok()
                   : BadRequest("Can't change currency round count");
    }
}
