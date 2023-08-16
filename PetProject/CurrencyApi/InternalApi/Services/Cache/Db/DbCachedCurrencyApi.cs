using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Db;

/// <inheritdoc />
public sealed class DbCachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly CurrencyInternalContext      _context;
    private readonly ILogger<DbCachedCurrencyApi> _logger;
    private readonly ICurrencyApiService          _apiService;
    private readonly int                          _cacheRelevanceHours;
    private readonly CurrenciesSettings           _settings;

    public DbCachedCurrencyApi(CurrencyInternalContext             context,
                               IOptionsMonitor<CacheSettings>      optionsMonitor,
                               ILogger<DbCachedCurrencyApi>        logger,
                               ICurrencyApiService                 apiService,
                               IOptionsMonitor<CurrenciesSettings> currenciesMonitor)
    {
        _context             = context;
        _logger              = logger;
        _apiService          = apiService;
        _cacheRelevanceHours = optionsMonitor.CurrentValue.CacheRelevanceHours;
        _settings            = currenciesMonitor.CurrentValue;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrentCurrencyAsync(CurrencyType      currencyType,
                                                            CancellationToken cancellationToken)
    {
        CurrencyInfo currencyInfo;
        if (InfoOutdated(out CurrenciesOnDateEntity? newestInfo))
        {
            _logger.LogDebug("Did not find relevant cache info");

            CurrencyInfo[] currenciesInfo =
                await _apiService.GetAllCurrentCurrenciesAsync(_settings.BaseCurrency, cancellationToken);
            currencyInfo = currenciesInfo.Single(currency => currency.Code == currencyType);

            DateTime now = DateTime.UtcNow;
            var currenciesOnDate = new CurrenciesOnDateEntity
                                   {
                                       LastUpdatedAt = now
                                   };
            foreach (CurrencyInfo info in currenciesInfo)
            {
                currenciesOnDate.Currencies.Add(new CurrencyInfoEntity
                                                {
                                                    Value     = info.Value,
                                                    Code      = info.Code,
                                                    UpdatedAt = now
                                                });
            }

            await _context.CurrenciesOnDates.AddAsync(currenciesOnDate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Saved info to cache at {@Date}", currenciesOnDate.LastUpdatedAt);

            return currencyInfo;
        }

        currencyInfo = new CurrencyInfo
                       {
                           Code  = currencyType,
                           Value = newestInfo!.Currencies.Single(entity => entity.Code == currencyType).Value
                       };
        _logger.LogDebug("Found relevant info in cache {@Model}", currencyInfo);

        return currencyInfo;
    }

    private bool InfoOutdated(out CurrenciesOnDateEntity? newestInfo)
    {
        newestInfo = _context.CurrenciesOnDates
                             .OrderByDescending(static entity => entity.LastUpdatedAt)
                             .Take(1)
                             .Include(static entity => entity.Currencies)
                             .SingleOrDefault();
        if (newestInfo is null)
        {
            return true;
        }

        DateTime updatedAt    = newestInfo.LastUpdatedAt;
        DateTime now          = DateTime.UtcNow;
        double   hoursElapsed = (now - updatedAt).TotalHours;

        return hoursElapsed > _cacheRelevanceHours;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyOnDateAsync(CurrencyType      currencyType,
                                                           DateOnly          date,
                                                           CancellationToken cancellationToken)
    {
        CurrencyInfo            currencyInfo;
        CurrenciesOnDateEntity? info = GetInfoOnDate(date);
        if (info is null)
        {
            _logger.LogDebug("Did not find relevant cache info for {Date}", date);

            CurrenciesOnDate onDate =
                await _apiService.GetAllCurrenciesOnDateAsync(_settings.BaseCurrency, date, cancellationToken);
            currencyInfo = onDate.Currencies.Single(currency => currency.Code == currencyType);

            var currenciesOnDate = new CurrenciesOnDateEntity
                                   {
                                       LastUpdatedAt = onDate.LastUpdatedAt
                                   };
            foreach (CurrencyInfo currency in onDate.Currencies)
            {
                currenciesOnDate.Currencies.Add(new CurrencyInfoEntity
                                                {
                                                    Value     = currency.Value,
                                                    Code      = currency.Code,
                                                    UpdatedAt = onDate.LastUpdatedAt
                                                });
            }

            await _context.CurrenciesOnDates.AddAsync(currenciesOnDate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Saved info to cache at {@Date}", currenciesOnDate.LastUpdatedAt);

            return currencyInfo;
        }

        currencyInfo = new CurrencyInfo
                       {
                           Code  = currencyType,
                           Value = info.Currencies.Single(entity => entity.Code == currencyType).Value
                       };
        _logger.LogDebug("Found relevant info in cache {@Model}", currencyInfo);

        return currencyInfo;
    }

    private CurrenciesOnDateEntity? GetInfoOnDate(DateOnly date)
    {
        return _context.CurrenciesOnDates
                       .Where(entity => DateOnly.FromDateTime(entity.LastUpdatedAt) == date)
                       .OrderByDescending(static entity => entity.LastUpdatedAt)
                       .Take(1)
                       .Include(static entity => entity.Currencies)
                       .SingleOrDefault();
    }
}
