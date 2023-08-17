using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Db;

/// <summary>
/// Предоставляет методы для работы с базой данных кэша.
/// </summary>
public sealed class DbCacheRepository
{
    private readonly CurrencyInternalContext   _context;
    private readonly ILogger<DbCacheRepository> _logger;
    private readonly int                       _cacheRelevanceHours;

    /// <summary>
    /// Создает экземпляр класса.
    /// </summary>
    /// <param name="context">Контекст базы данных кэша.</param>
    /// <param name="optionsMonitor">Настройки приложения (доступ ко времени действия кэша).</param>
    /// <param name="logger">Логгер.</param>
    public DbCacheRepository(CurrencyInternalContext        context,
                             IOptionsMonitor<CacheSettings> optionsMonitor,
                             ILogger<DbCacheRepository>      logger)
    {
        _context             = context;
        _logger              = logger;
        _cacheRelevanceHours = optionsMonitor.CurrentValue.CacheRelevanceHours;
    }

    /// <summary>
    /// Проверка кэша на устаревание.
    /// </summary>
    /// <param name="newestInfo">Новейшая информация кэша.</param>
    /// <returns>true – кэш устарел, false – не устарел.</returns>
    internal bool InfoOutdated(out CurrenciesOnDateEntity? newestInfo)
    {
        newestInfo = _context.CurrenciesOnDates.GetNewest();
        if (newestInfo is null)
        {
            return true;
        }

        DateTime updatedAt    = newestInfo.LastUpdatedAt;
        DateTime now          = DateTime.UtcNow;
        double   hoursElapsed = (now - updatedAt).TotalHours;

        return hoursElapsed > _cacheRelevanceHours;
    }

    /// <summary>
    /// Сохранение информации о валютах в кэш.
    /// </summary>
    /// <param name="dateTime">Дата и время полученной информации.</param>
    /// <param name="currenciesInfo">Коллекция валют.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    internal async Task SaveCurrenciesOnDateAsync(DateTime                  dateTime,
                                                  IEnumerable<CurrencyInfo> currenciesInfo,
                                                  CancellationToken         cancellationToken)
    {

        var currenciesOnDate = new CurrenciesOnDateEntity
                               {
                                   LastUpdatedAt = dateTime
                               };
        foreach (CurrencyInfo info in currenciesInfo)
        {
            currenciesOnDate.Currencies.Add(new CurrencyInfoEntity
                                            {
                                                Value     = info.Value,
                                                Code      = info.Code,
                                                UpdatedAt = dateTime
                                            });
        }

        await _context.CurrenciesOnDates.AddAsync(currenciesOnDate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Saved info to cache at {@Date}", currenciesOnDate.LastUpdatedAt);
    }

    /// <summary>
    /// Получение информации из кэша на определенную дату.
    /// </summary>
    /// <param name="date">Дата, на которую получена информация.</param>
    /// <returns>Сущность информации о валютах на дату.</returns>
    internal CurrenciesOnDateEntity? GetInfoOnDate(DateOnly date)
    {
        return _context.CurrenciesOnDates
                       .Where(entity => DateOnly.FromDateTime(entity.LastUpdatedAt) == date)
                       .GetNewest();
    }
}

file static class Extensions
{
    public static CurrenciesOnDateEntity? GetNewest(this IQueryable<CurrenciesOnDateEntity> entities)
    {
        return entities.OrderByDescending(static entity => entity.LastUpdatedAt)
                       .Take(1)
                       .Include(static entity => entity.Currencies)
                       .SingleOrDefault();
    }
}
