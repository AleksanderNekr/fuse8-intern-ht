using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

/// <inheritdoc />
public class CachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly ICurrencyAPI               _currencyApi;
    private readonly CacheWorkerService         _cacheService;
    private readonly CurrenciesSettings         _settings;
    private readonly ILogger<CachedCurrencyApi> _logger;

    /// <inheritdoc cref="ICachedCurrencyAPI" />
    public CachedCurrencyApi(ICurrencyAPI                        currencyApi,
                             IOptionsMonitor<CurrenciesSettings> currenciesMonitor,
                             CacheWorkerService                  cacheService,
                             ILogger<CachedCurrencyApi>          logger)
    {
        _currencyApi  = currencyApi;
        _cacheService = cacheService;
        _settings     = currenciesMonitor.CurrentValue;
        _logger       = logger;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrentCurrencyAsync(CurrencyType      currencyType,
                                                            CancellationToken cancellationToken)
    {
        _cacheService.UpdateCacheInfo();
        FileInfo? newestFile = _cacheService.TryGetNewestFile();

        CurrencyInfo[] currenciesInfo;
        CurrencyInfo   currencyInfo;
        if (newestFile is null || _cacheService.CacheIsOlderThan(_settings.CacheRelevanceHours))
        {
            _logger.LogDebug("Did not find relevant cache file");

            currenciesInfo = await _currencyApi.GetAllCurrentCurrenciesAsync(_settings.BaseCurrency, cancellationToken);
            currencyInfo   = currenciesInfo.Single(currency => currency.Code == currencyType);

            await _cacheService.SaveToCache(DateTime.Now, currenciesInfo, cancellationToken);

            return currencyInfo;
        }

        currenciesInfo = await _cacheService.GetFromCache(newestFile, cancellationToken);
        currencyInfo   = currenciesInfo.Single(currency => currency.Code == currencyType);

        return currencyInfo;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyOnDateAsync(CurrencyType      currencyType,
                                                           DateOnly          date,
                                                           CancellationToken cancellationToken)
    {
        _cacheService.UpdateCacheInfo();
        FileInfo? relevantFile = _cacheService.TryGetFileOnDate(date);

        CurrencyInfo currencyInfo;
        if (relevantFile is null)
        {
            _logger.LogDebug("Did not find relevant cache file");
            CurrenciesOnDate currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(
                                                     _settings.BaseCurrency,
                                                     date,
                                                     cancellationToken);

            await _cacheService.SaveToCache(currenciesOnDate.LastUpdatedAt,
                                            currenciesOnDate.Currencies,
                                            cancellationToken);
            currencyInfo = currenciesOnDate.Currencies.Single(currency => currency.Code == currencyType);

            return currencyInfo;
        }

        CurrencyInfo[] currencies = await _cacheService.GetFromCache(relevantFile, cancellationToken);
        currencyInfo = currencies.Single(currency => currency.Code == currencyType);

        return currencyInfo;
    }
}
