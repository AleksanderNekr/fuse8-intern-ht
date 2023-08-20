using Fuse8_ByteMinds.SummerSchool.Grpc;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

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
            _logger.LogTrace("Executed GET all favorites");
            IEnumerable<FavoriteExchangeRateEntity> favorites = _context.FavoriteExchangeRates.AsEnumerable();
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogTrace("Received favorites from DB: {Favorite}", favorites.ToJson());

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
            _logger.LogTrace("Executed GET favorite");
            FavoriteExchangeRateEntity? favorite =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            _logger.LogTrace("Received favorite from DB: {Favorite}", favorite.ToJson());

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
            _logger.LogTrace("Executed POST new favorite");
            bool nameTaken = await CheckIfNameTakenAsync(name, cancellationToken);
            if (nameTaken)
            {
                _logger.LogInformation("Conflict: Favorite with name ({Name}) already exists", name);

                return Conflict($"Favorite with name ({name}) already exists");
            }

            _logger.LogTrace("Name {Name} – unique", name);

            bool currenciesPairExists = await _context.FavoriteExchangeRates.AnyAsync(entity =>
                                                entity.Currency     == currency
                                             && entity.BaseCurrency == baseCurrency,
                                            cancellationToken);
            if (currenciesPairExists)
            {
                _logger.LogInformation("Conflict: Favorite with currency {Currency} and base currency {BaseCurrency} already exists",
                                       currency,
                                       baseCurrency);

                return Conflict($"Favorite with currency {currency} and base currency {baseCurrency} already exists");
            }

            _logger.LogTrace("Pair {Currency}|{BaseCurrency} – unique", currency, baseCurrency);

            FavoriteExchangeRateEntity favorite = new()
                                                  {
                                                      Name         = name,
                                                      Currency     = currency,
                                                      BaseCurrency = baseCurrency
                                                  };

            await _context.FavoriteExchangeRates.AddAsync(favorite, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added new favorite {Favorite}", favorite.ToJson());

            return Ok(favorite);
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
            _logger.LogTrace("Executed PUT favorite");
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);

            if (found is null)
            {
                _logger.LogWarning("Favorite with name {Name} not found", name);

                return NotFound($"Favorite with name {name} not found");
            }

            _logger.LogTrace("Received favorite from DB: {Favorite}", found.ToJson());

            if (!ChangesDetected())
            {
                return Ok(found);
            }

            if (NameChanged())
            {
                bool nameTaken = await CheckIfNameTakenAsync(newName!, cancellationToken);
                if (nameTaken)
                {
                    return Conflict("Favorite with new name already exists");
                }

                found.Name = newName!;

                if (OnlyNameProvided())
                {
                    _context.Update(found);
                    await _context.SaveChangesAsync(cancellationToken);

                    return Ok(found);
                }
            }

            if (CurrencyChanged())
            {
                found.Currency = newCurrency!.Value;
            }

            if (BaseCurrencyChanged())
            {
                found.BaseCurrency = newBaseCurrency!.Value;
            }

            bool anotherPairFound = await _context.FavoriteExchangeRates.AnyAsync(entity =>
                                            entity.Currency     == found.Currency
                                         && entity.BaseCurrency == found.BaseCurrency
                                         && entity.Name         != found.Name,
                                        cancellationToken);
            if (anotherPairFound)
            {
                _logger.LogInformation("Favorite with new currency {Currency} and base currency {BaseCurrency} already exists",
                                       found.Currency,
                                       found.BaseCurrency);

                return Conflict($"Favorite with new currency {found.Currency}"
                              + $" and base currency {found.BaseCurrency} already exists");
            }


            _context.Update(found);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Favorite updated {Favorite}", found.ToJson());

            return Ok(found);

            bool ChangesDetected()
            {
                bool changed = NameChanged() || CurrencyChanged() || BaseCurrencyChanged();
                if (!changed)
                {
                    _logger.LogDebug("No changes found");
                }

                return changed;
            }

            bool OnlyNameProvided()
            {
                bool nameOnly = newName is not null && newCurrency is null && newBaseCurrency is null;
                if (nameOnly)
                {
                    _logger.LogInformation("Only name {Name} provided to update", newName);
                }
                else
                {
                    _logger.LogTrace("Not only name provided");
                }

                return nameOnly;
            }

            bool NameChanged()
            {
                bool changed = newName is not null && found.Name != newName;
                if (changed)
                {
                    _logger.LogTrace("New name {NewName} differs to old: {OldName}", newName, found.Name);
                }
                else
                {
                    _logger.LogTrace("New name didn't change");
                }

                return changed;
            }

            bool CurrencyChanged()
            {
                bool changed = newCurrency is not null && newCurrency != found.Currency;
                if (changed)
                {
                    _logger.LogTrace("New currency {NewCurrency} differs to old: {OldCurrency}",
                                     newCurrency,
                                     found.Currency);
                }
                else
                {
                    _logger.LogTrace("New currency didn't change");
                }

                return changed;
            }

            bool BaseCurrencyChanged()
            {
                bool changed = newBaseCurrency is not null && newBaseCurrency != found.BaseCurrency;
                if (changed)
                {
                    _logger.LogTrace("New base currency {NewBaseCurrency} differs to old: {OldBaseCurrency}",
                                     newBaseCurrency,
                                     found.Currency);
                }
                else
                {
                    _logger.LogTrace("New base currency didn't change");
                }

                return changed;
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
            _logger.LogTrace("Executed DELETE favorite");
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            if (found is null)
            {
                _logger.LogInformation("Favorite with name {Name} not found", name);

                return NotFound($"Favorite with name {name} not found");
            }

            string jsonFound = found.ToJson();
            _logger.LogTrace("Received favorite from DB: {Favorite}", jsonFound);

            _context.FavoriteExchangeRates.Remove(found);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Favorite {Favorite} deleted", jsonFound);

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
        public async Task<ActionResult<CurrencyInfo>> GetCurrentFavoriteAsync(
            string name, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Executed GET favorite currency info");
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            _ = found ?? throw new InvalidFavoriteNameException($"Favorite with name {name} not found");
            _logger.LogTrace("Received favorite from DB: {Favorite}", found.ToJson());

            Task<CurrenciesSettings> getSettingsTask = _context.Settings.SingleAsync(cancellationToken);
            _logger.LogTrace("Executed get settings task");

            CurrencyFavoriteRequest request = new()
                                              {
                                                  FavoriteCurrency     = (CurrencyCode)found.Currency,
                                                  FavoriteBaseCurrency = (CurrencyCode)found.BaseCurrency
                                              };
            _logger.LogTrace("Sent request: {Request}", request);
            CurrencyResponse response =
                await _grpcService.GetCurrentFavoriteCurrencyAsync(request, cancellationToken: cancellationToken);

            CurrenciesSettings settings = await getSettingsTask;
            _logger.LogTrace("Received settings: {Settings}", settings);

            CurrencyInfo info = new()
                                {
                                    Code  = found.Currency,
                                    Value = Math.Round(response.Value, settings.DecimalPlace)
                                };
            _logger.LogTrace("Sent currency info: {Info}", info);

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
        public async Task<ActionResult<CurrencyOnDateInfo>> GetFavoriteOnDateAsync(
            string               name,
            [FromQuery] DateOnly date,
            CancellationToken    cancellationToken)
        {
            _logger.LogTrace("Executed GET favorite currency on date info");
            FavoriteExchangeRateEntity? found =
                await _context.FavoriteExchangeRates.SingleOrDefaultAsync(entity => entity.Name == name,
                                                                          cancellationToken);
            _ = found ?? throw new InvalidFavoriteNameException($"Favorite with name {name} not found");
            _logger.LogTrace("Received favorite from DB: {Favorite}", found.ToJson());

            Task<CurrenciesSettings> getSettingsTask = _context.Settings.SingleAsync(cancellationToken);
            _logger.LogTrace("Executed get settings task");

            CurrencyOnDateFavoriteRequest request = new()
                                                    {
                                                        FavoriteCurrency     = (CurrencyCode)found.Currency,
                                                        FavoriteBaseCurrency = (CurrencyCode)found.BaseCurrency,
                                                        Date = Timestamp.FromDateTime(
                                                         date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
                                                    };
            _logger.LogTrace("Sent request: {Request}", request);
            CurrencyResponse response =
                await _grpcService.GetFavoriteCurrencyOnDateAsync(request, cancellationToken: cancellationToken);

            CurrenciesSettings settings = await getSettingsTask;
            _logger.LogTrace("Received settings: {Settings}", settings);

            CurrencyOnDateInfo info = new()
                                      {
                                          Code  = found.Currency,
                                          Value = Math.Round(response.Value, settings.DecimalPlace),
                                          Date  = date
                                      };
            _logger.LogTrace("Sent currency info: {Info}", info);

            return info;
        }

        private async Task<bool> CheckIfNameTakenAsync(string name, CancellationToken cancellationToken)
        {
            bool taken = await _context.FavoriteExchangeRates.AnyAsync(entity => entity.Name == name, cancellationToken);
            if (taken)
            {
                _logger.LogWarning("Favorite with name {Name} already exists", name);
            }
            else
            {
                _logger.LogTrace("Name {Name} – unique", name);
            }

            return taken;
        }
    }
}

internal sealed class InvalidFavoriteNameException : Exception
{
    internal InvalidFavoriteNameException(string message) : base(message)
    {
    }
}
