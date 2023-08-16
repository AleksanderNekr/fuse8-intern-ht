using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Local;

/// <inheritdoc />
public class LocalCachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly ICurrencyApiService        _currencyApi;
    private readonly LocalCacheWorkerService         _cacheService;
    private readonly CurrenciesSettings         _settings;
    private readonly ILogger<LocalCachedCurrencyApi> _logger;

    /// <inheritdoc cref="ICachedCurrencyAPI" />
    public LocalCachedCurrencyApi(ICurrencyApiService                 currencyApi,
                             IOptionsMonitor<CurrenciesSettings> currenciesMonitor,
                             LocalCacheWorkerService                  cacheService,
                             ILogger<LocalCachedCurrencyApi>          logger)
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
        if (newestFile is null || _cacheService.CacheOutdated())
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
