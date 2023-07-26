using System.Net;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

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
    public CurrencyApiController(IHttpClientFactory           clientFactory,
                                 IOptions<CurrenciesSettings> currenciesSettings)
    {
        _httpClient = clientFactory.CreateClient("DefaultClient");

        _baseCurrency    = currenciesSettings.Value.BaseCurrency;
        _defaultCurrency = currenciesSettings.Value.DefaultCurrency;
        _decimalPlace    = currenciesSettings.Value.DecimalPlace;
    }

    /// <summary>
    ///     Получение курса валюты по умолчанию.
    /// </summary>
    /// <response code="200">
    ///     Возвращает, если удалось получить курс валюты по умолчанию от api.currencyapi.com.
    /// </response>
    /// <response code="400">
    ///     Возвращает, если не удалось получить курс валюты по умолчанию от api.currencyapi.com.
    /// </response>
    /// <returns>
    ///     JSON
    ///     <example>
    ///         {
    ///         "code": "RUB",
    ///         "value": 90.50
    ///         }
    ///     </example>
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetDefaultCurrency(CancellationToken stopToken)
    {
        var requestUri = $"latest?currencies={_defaultCurrency}&base_currency={_baseCurrency}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, stopToken);

        if (response.IsSuccessStatusCode)
        {
            string  responseBody    = await response.Content.ReadAsStringAsync(stopToken);
            dynamic responseParsed  = JObject.Parse(responseBody);
            JObject dataSection     = responseParsed.data;
            var     currencySection = dataSection.Value<dynamic>(_defaultCurrency)!;
            decimal value           = currencySection.value;
            decimal roundedValue    = Math.Round(value, _decimalPlace);

            return new JsonResult(new
                                  {
                                      code  = _defaultCurrency,
                                      value = roundedValue,
                                  });
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            throw new CurrencyNotFoundException(nameof(_defaultCurrency), _defaultCurrency);
        }

        throw new BadHttpRequestException(response.Headers.ToString());
    }
}
