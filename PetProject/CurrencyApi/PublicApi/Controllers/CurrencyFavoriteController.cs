using Fuse8_ByteMinds.SummerSchool.Grpc;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers
{
    /// <summary>
    ///     Предоставляет методы работы с избранными курсами валют.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CurrencyFavoriteController : ControllerBase
    {
        private readonly CurrencyPublicContext                 _context;
        private readonly ILogger<CurrencyFavoriteController>   _logger;
        private readonly CurrencyApiGrpc.CurrencyApiGrpcClient _grpcService;

        public CurrencyFavoriteController(CurrencyPublicContext                 context,
                                          ILogger<CurrencyFavoriteController>   logger,
                                          CurrencyApiGrpc.CurrencyApiGrpcClient grpcService)
        {
            _context     = context;
            _logger      = logger;
            _grpcService = grpcService;
        }

        /// <summary>
        ///     Получение списка всех избранных курсов валют.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Результат со списком сущностей валют.</returns>
        /// <response code="200">
        ///     Возвращает, если удалось получить избранное.
        /// </response>
        /// <response code="400">
        ///     Возвращает, если не удалось получить избранное.
        /// </response>
        [HttpGet]
        public Task<ActionResult<IEnumerable<FavoriteExchangeRateEntity>>> GetAllFavoritesAsync(
            CancellationToken cancellationToken)
        {
            IEnumerable<FavoriteExchangeRateEntity> favorites = _context.FavoriteExchangeRates.AsEnumerable();
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(new ActionResult<IEnumerable<FavoriteExchangeRateEntity>>(favorites));
        }

        /// <summary>
        ///     Получение избранного по названию.
        /// </summary>
        /// <param name="name">Название избранного.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Найденное избранное</returns>
        /// <response code="200">
        ///     Возвращает, если удалось получить избранное.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если не удалось найти избранное.
        /// </response>
        /// <exception cref="InvalidFavoriteNameException">Не удалось найти избранное по данному имени.</exception>
        [HttpGet("{name}")]
        public async Task<ActionResult<FavoriteExchangeRateEntity>> GetByNameAsync(
            string name, CancellationToken cancellationToken)
        {
            FavoriteExchangeRateEntity? favorite =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);

            return favorite
                ?? throw new InvalidFavoriteNameException("Can't find favorite for this name: " + name);
        }

        /// <summary>
        ///     Добавление нового избранного курса валют.
        /// </summary>
        /// <param name="name">Название избранного.</param>
        /// <param name="currency">Валюта для конвертации.</param>
        /// <param name="baseCurrency">Базовая валюта.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>
        ///     <see cref="OkResult" /> – удалось добавить, <see cref="ConflictResult" /> – не удалось добавить в результате
        ///     конфликта с другим избранным.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Ворзникает при неизвестном результате добавления.</exception>
        /// <response code="200">
        ///     Возвращает, если удалось добавить.
        /// </response>
        /// <response code="409">
        ///     Возвращает, если не удалось добавить из-за конфликта с другим избранным.
        /// </response>
        [HttpPost]
        public async Task<IActionResult> PostFavoriteAsync([FromQuery] string       name,
                                                           [FromQuery] CurrencyType currency,
                                                           [FromQuery] CurrencyType baseCurrency,
                                                           CancellationToken        cancellationToken)
        {
            _logger.LogDebug("Executed POST new favorite");
            bool nameTaken = await CheckIfNameTakenAsync(name, cancellationToken);
            if (nameTaken)
            {
                return Conflict($"Favorite with name ({name}) already exists");
            }

            bool currenciesPairExists = await _context.FavoriteExchangeRates.AnyAsync(entity =>
                                                         entity.Currency     == currency
                                                      && entity.BaseCurrency == baseCurrency,
                                                 cancellationToken);
            if (currenciesPairExists)
            {
                return Conflict($"Favorite with currency {currency} and base currency {baseCurrency} already exists");
            }

            FavoriteExchangeRateEntity favorite = new()
                                                  {
                                                      Name         = name,
                                                      Currency     = currency,
                                                      BaseCurrency = baseCurrency
                                                  };

            await _context.FavoriteExchangeRates.AddAsync(favorite, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved new favorite {@Fav}", favorite);

            return Ok();
        }

        /// <summary>
        ///     Изменение существующего избранного курса валют.
        /// </summary>
        /// <param name="name">Название избранного для изменения.</param>
        /// <param name="newName">Новое название избранного (необязательно) – если не заполнено, остается прежним.</param>
        /// <param name="newCurrency">Новая валюта для конвертации (необязательно) – если не заполнено, остается прежним.</param>
        /// <param name="newBaseCurrency">Новая базовая валюта (необязательно) – если не заполнено, остается прежним.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>
        ///     <see cref="OkResult" /> – удалось обновить, <see cref="ConflictResult" /> – не удалось обновить
        ///     в результате конфликта с другим избранным, <see cref="NotFoundResult" /> – не удалось найти избранное по имени.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Ворзникает при неизвестном результате добавления.</exception>
        /// <response code="200">
        ///     Возвращает, если удалось обновить.
        /// </response>
        /// <response code="409">
        ///     Возвращает, если не удалось обновить из-за конфликта с другим избранным.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если не удалось найти избранное для обновления.
        /// </response>
        [HttpPut("{name}")]
        public async Task<IActionResult> UpdateFavoriteAsync(string                    name,
                                                             CancellationToken         cancellationToken,
                                                             [FromQuery] string?       newName         = null,
                                                             [FromQuery] CurrencyType? newCurrency     = null,
                                                             [FromQuery] CurrencyType? newBaseCurrency = null)
        {
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            if (found is null)
            {
                return NotFound($"Favorite with name {name} not found");
            }

            if (NoChanges())
            {
                return Ok("No changes provided");
            }

            if (NameChanged())
            {
                bool nameTaken = await CheckIfNameTakenAsync(newName!, cancellationToken);
                if (nameTaken)
                {
                    return Conflict("Favorite with new name already exists");
                }

                found.Name = newName!;
                _logger.LogDebug("Name updated");

                if (OnlyNameProvided())
                {
                    _context.Update(found);
                    await _context.SaveChangesAsync(cancellationToken);

                    return Ok("Only name was updated");
                }
            }

            if (CurrencyChanged())
            {
                found.Currency = newCurrency!.Value;
                _logger.LogDebug("Currency updated");
            }

            if (BaseCurrencyChanged())
            {
                found.BaseCurrency = newBaseCurrency!.Value;
                _logger.LogDebug("Base currency updated");
            }

            bool anotherPairFound = await _context.FavoriteExchangeRates.AnyAsync(entity =>
                                                     entity.Currency     == found.Currency
                                                  && entity.BaseCurrency == found.BaseCurrency
                                                  && entity.Name         != found.Name,
                                             cancellationToken);
            if (anotherPairFound)
            {
                return Conflict($"Favorite with new currency {found.Currency}"
                              + $" and base currency {found.BaseCurrency} already exists");
            }


            _context.Update(found);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Favorite updated {@Fav}", found);

            return Ok();

            bool NoChanges()
            {
                return !(NameChanged() || CurrencyChanged() || BaseCurrencyChanged());
            }

            bool OnlyNameProvided()
            {
                return newName is not null && newCurrency is null && newBaseCurrency is null;
            }

            bool NameChanged()
            {
                return newName is not null && found.Name != newName;
            }

            bool CurrencyChanged()
            {
                return newCurrency is not null && newCurrency != found.Currency;
            }

            bool BaseCurrencyChanged()
            {
                return newBaseCurrency is not null && newBaseCurrency != found.BaseCurrency;
            }
        }


        /// <summary>
        /// Удаление избранного по имени.
        /// </summary>
        /// <param name="name">Название избранного для удаления.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><see cref="OkResult"/> – удаление прошло успешно, <see cref="NotFoundResult"/> – не удалось найти избранное по имени.</returns>
        /// <response code="200">
        ///     Возвращает, если удалось удалить.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если не удалось найти избранное для удаления.
        /// </response>
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteFavoriteAsync(string name, CancellationToken cancellationToken)
        {
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            if (found is null)
            {
                return NotFound($"Favorite with name {name} not found");
            }

            _context.FavoriteExchangeRates.Remove(found);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok();
        }

        /// <summary>
        /// Получение текущего значения избранного курса валют.
        /// </summary>
        /// <param name="name">Название избранного курса валют.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Модель информации о курсе валюты.</returns>
        /// <exception cref="InvalidFavoriteNameException">Возникает, если избранное с приведенным именем не найдено.</exception>
        /// <response code="200">
        ///     Возвращает, если удалось получить значение избранного курса валют.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если не удалось найти избранное.
        /// </response>
        [HttpGet("current/{name}")]
        public async Task<CurrencyInfo> GetCurrentFavoriteAsync(string name, CancellationToken cancellationToken)
        {
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            _ = found ?? throw new InvalidFavoriteNameException($"Favorite with name {name} not found");
            Task<CurrenciesSettings> getSettingsTask = _context.Settings.SingleAsync(cancellationToken);

            CurrencyFavoriteRequest request = new()
                                              {
                                                  FavoriteCurrency     = (CurrencyCode)found.Currency,
                                                  FavoriteBaseCurrency = (CurrencyCode)found.BaseCurrency
                                              };
            CurrencyResponse response =
                await _grpcService.GetCurrentFavoriteCurrencyAsync(request, cancellationToken: cancellationToken);
            CurrenciesSettings settings = await getSettingsTask;
            CurrencyInfo info = new()
                                {
                                    Code  = found.Currency,
                                    Value = Math.Round(response.Value, settings.DecimalPlace)
                                };

            return info;
        }

        /// <summary>
        /// Получение значения избранного курса валют на определенную дату.
        /// </summary>
        /// <param name="name">Название избранного курса валют.</param>
        /// <param name="date">Дата, на которую должен быть получен курс.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Модель информации о курсе валюты.</returns>
        /// <exception cref="InvalidFavoriteNameException">Возникает, если избранное с приведенным именем не найдено.</exception>
        /// <response code="200">
        ///     Возвращает, если удалось получить значение избранного курса валют.
        /// </response>
        /// <response code="404">
        ///     Возвращает, если не удалось найти избранное.
        /// </response>
        [HttpGet("historical/{name}")]
        public async Task<CurrencyOnDateInfo> GetFavoriteOnDateAsync(string               name,
                                                                     [FromQuery] DateOnly date,
                                                                     CancellationToken    cancellationToken)
        {
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            _ = found ?? throw new InvalidFavoriteNameException($"Favorite with name {name} not found");
            Task<CurrenciesSettings> getSettingsTask = _context.Settings.SingleAsync(cancellationToken);

            CurrencyOnDateFavoriteRequest request = new()
                                                    {
                                                        FavoriteCurrency     = (CurrencyCode)found.Currency,
                                                        FavoriteBaseCurrency = (CurrencyCode)found.BaseCurrency,
                                                        Date = Timestamp.FromDateTime(
                                                             date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
                                                    };
            CurrencyResponse response =
                await _grpcService.GetFavoriteCurrencyOnDateAsync(request, cancellationToken: cancellationToken);
            CurrenciesSettings settings = await getSettingsTask;
            CurrencyOnDateInfo info = new()
                                      {
                                          Code  = found.Currency,
                                          Value = Math.Round(response.Value, settings.DecimalPlace),
                                          Date  = date
                                      };

            return info;
        }

        private Task<bool> CheckIfNameTakenAsync(string name, CancellationToken cancellationToken)
        {
            return _context.FavoriteExchangeRates.AnyAsync(entity => entity.Name == name, cancellationToken);
        }
    }
}

internal sealed class InvalidFavoriteNameException : Exception
{
    internal InvalidFavoriteNameException(string message) : base(message)
    {
    }
}
