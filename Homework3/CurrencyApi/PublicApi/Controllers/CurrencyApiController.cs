using System.Net;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers
{
    /// <summary>
    /// Контроллер для работы с курсами валют.
    /// </summary>
    [Route("currency")]
    [ApiController]
    public class CurrencyApiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string     _baseCurrency;
        private readonly string     _defaultCurrency;
        private readonly int        _decimalPlace;

        /// <summary>
        /// Инициализация контроллера курсов валют.
        /// </summary>
        /// <param name="clientFactory">Фабрика создания HTTP-клиента.</param>
        /// <param name="currenciesSettings">Настройки получения информации о валютах.</param>
        public CurrencyApiController(IHttpClientFactory           clientFactory,
                                     IOptions<CurrenciesSettings> currenciesSettings)
        {
            this._httpClient = clientFactory.CreateClient("DefaultClient");

            this._baseCurrency    = currenciesSettings.Value.BaseCurrency;
            this._defaultCurrency = currenciesSettings.Value.DefaultCurrency;
            this._decimalPlace    = currenciesSettings.Value.DecimalPlace;
        }

        /// <summary>
        /// Получение курса валюты по умолчанию.
        /// </summary>
        /// <response code="200">
        /// Возвращает, если удалось получить курс валюты по умолчанию от api.currencyapi.com.
        /// </response>
        /// <response code="400">
        /// Возвращает, если не удалось получить курс валюты по умолчанию от api.currencyapi.com.
        /// </response>
        /// <returns>
        /// JSON
        /// <example>
        /// {
        ///    "code": "RUB",
        ///    "value": 90.50
        /// }
        /// </example>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetDefaultCurrency(CancellationToken stopToken)
        {
            var requestUri = $"latest?currencies={this._defaultCurrency}&base_currency={this._baseCurrency}";
            HttpResponseMessage response = await this._httpClient.GetAsync(requestUri, stopToken);

            if (response.IsSuccessStatusCode)
            {
                string  responseBody    = await response.Content.ReadAsStringAsync(stopToken);
                dynamic responseParsed  = JObject.Parse(responseBody);
                JObject dataSection     = responseParsed.data;
                var     currencySection = dataSection.Value<dynamic>(this._defaultCurrency)!;
                decimal roundedValue    = Math.Round(currencySection.value, this._decimalPlace);

                return new JsonResult(new
                                      {
                                          code  = this._defaultCurrency,
                                          value = roundedValue
                                      });
            }

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                throw new CurrencyNotFoundException(nameof(this._defaultCurrency), this._defaultCurrency);
            }

            throw new BadHttpRequestException(response.Headers.ToString());
        }
    }
}
