using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers
{
    /// <summary>
    /// Предоставляет методы для получения информации о валютах с использованием кэширования.
    /// </summary>
    [Route("[controller]/{currencyCode}")]
    [ApiController]
    public class CachedCurrencyApiController : ControllerBase
    {
        private readonly ICachedCurrencyAPI _service;

        /// <inheritdoc />
        public CachedCurrencyApiController(ICachedCurrencyAPI cachedCurrencyApi)
        {
            _service = cachedCurrencyApi;
        }

        /// <summary>
        /// Получение информации о валюте.
        /// </summary>
        /// <param name="currencyCode">Код валюты.</param>
        /// <param name="stopToken">Токен отмены операции.</param>
        /// <returns>Модель <see cref="CurrencyInfo"/>.</returns>
        [HttpGet("")]
        public async Task<ActionResult<CurrencyInfo>> GetCurrency(CurrencyType currencyCode, CancellationToken stopToken)
        {
            CurrencyInfo currencyInfo = await _service.GetCurrentCurrencyAsync(currencyCode, stopToken);

            return currencyInfo;
        }

        /// <summary>
        /// Получение информации о курсе валюты на определенную дату.
        /// </summary>
        /// <param name="currencyCode">Код валюты.</param>
        /// <param name="date">Дата курса валют.</param>
        /// <param name="stopToken">Токен отмены операции.</param>
        /// <returns>Модель <see cref="CurrencyInfo"/>.</returns>
        [HttpGet("{date}")]
        public async Task<ActionResult<CurrencyInfo>> GetCurrencyOnDate(CurrencyType      currencyCode,
                                                                        DateOnly          date,
                                                                        CancellationToken stopToken)
        {
            CurrencyInfo currencyInfo = await _service.GetCurrencyOnDateAsync(currencyCode, date, stopToken);

            return currencyInfo;
        }
    }
}