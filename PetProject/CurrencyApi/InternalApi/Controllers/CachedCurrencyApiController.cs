using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.BackgroundQueue;
using Microsoft.AspNetCore.Mvc;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers
{
    /// <summary>
    /// Предоставляет методы для получения информации о валютах с использованием кэширования.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CachedCurrencyApiController : ControllerBase
    {
        private readonly ICachedCurrencyAPI                   _service;
        private readonly IBackgroundTaskQueue                 _tasksQueue;
        private readonly CurrencyInternalContext              _context;
        private readonly ILogger<CachedCurrencyApiController> _logger;

        /// <inheritdoc />
        public CachedCurrencyApiController(ICachedCurrencyAPI                   cachedCurrencyApi,
                                           IBackgroundTaskQueue                 tasksQueue,
                                           CurrencyInternalContext              context,
                                           ILogger<CachedCurrencyApiController> logger)
        {
            _service    = cachedCurrencyApi;
            _tasksQueue = tasksQueue;
            _context    = context;
            _logger     = logger;
        }

        /// <summary>
        /// Получение информации о валюте.
        /// </summary>
        /// <param name="currencyCode">Код валюты.</param>
        /// <param name="stopToken">Токен отмены операции.</param>
        /// <returns>Модель <see cref="CurrencyInfo"/>.</returns>
        /// <response code="200">
        ///     Возвращает, если удалось получить курс валюты.
        /// </response>
        /// <response code="400">
        ///     Возвращает, если не удалось получить курс валюты.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если валюта не найдена.
        /// </response>
        [HttpGet("{currencyCode}")]
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
        /// <response code="200">
        ///     Возвращает, если удалось получить курс валюты.
        /// </response>
        /// <response code="400">
        ///     Возвращает, если не удалось получить курс валюты.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если валюта не найдена.
        /// </response>
        [HttpGet("{currencyCode}/{date}")]
        public async Task<ActionResult<CurrencyInfo>> GetCurrencyOnDate(CurrencyType      currencyCode,
                                                                        DateOnly          date,
                                                                        CancellationToken stopToken)
        {
            CurrencyInfo currencyInfo = await _service.GetCurrencyOnDateAsync(currencyCode, date, stopToken);

            return currencyInfo;
        }

        /// <summary>
        /// Пересчет кэша относительно новой базовой валюты.
        /// </summary>
        /// <param name="newBaseCurrency">Новая базовая валюта.</param>
        /// <param name="stopToken">Токен отмены операции.</param>
        /// <returns>ID созданной задачи на обновление.</returns>
        /// <response code="202">
        ///     Возвращает, если удалось создать задачу на обновление.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если новая валюта некорректна.
        /// </response>
        [HttpPatch("recalc_against/{newBaseCurrency}")]
        public async Task<ActionResult<Guid>> RecalculateCacheAgainstAsync(
            CurrencyType newBaseCurrency, CancellationToken stopToken)
        {
            var          id     = Guid.NewGuid();
            const Status status = Status.Created;

            CacheTaskEntity task = new()
                                   {
                                       Id              = id,
                                       Status          = status,
                                       NewBaseCurrency = newBaseCurrency
                                   };

            AddTaskResult result = await _tasksQueue.EnqueueAsync(task, stopToken);

            if (result != AddTaskResult.Success)
            {
                _logger.LogError("Bad try to enqueue task {Task}.\nResult: {Result}", task, result);

                return NotFound();
            }

            await _context.AddAsync(task, stopToken);
            await _context.SaveChangesAsync(stopToken);
            _logger.LogInformation("Successfully enqueued and published task {Task}", task);

            return Accepted(id);
        }
    }
}
