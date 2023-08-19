using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Db;

/// <inheritdoc />
public sealed class DbCachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly ILogger<DbCachedCurrencyApi> _logger;
    private readonly ICurrencyApiService          _apiService;
    private readonly DbCacheRepository            _repository;
    private readonly CurrenciesSettings           _settings;

    /// <inheritdoc cref="ICachedCurrencyAPI" />
    public DbCachedCurrencyApi(ILogger<DbCachedCurrencyApi>        logger,
                               ICurrencyApiService                 apiService,
                               IOptionsMonitor<CurrenciesSettings> currenciesMonitor,
                               DbCacheRepository                   repository)
    {
        _logger     = logger;
        _apiService = apiService;
        _repository = repository;
        _settings   = currenciesMonitor.CurrentValue;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrentCurrencyAsync(CurrencyType      currencyType,
                                                            CancellationToken cancellationToken)
    {
        if (_repository.InfoOutdated(out CurrenciesOnDateEntity? newestInfo))
        {
            _logger.LogDebug("Did not find relevant cache info");

            CurrencyInfo[] currenciesInfo =
                await _apiService.GetAllCurrentCurrenciesAsync(_settings.BaseCurrency, cancellationToken);

            DateTime now = DateTime.UtcNow;
            await _repository.SaveCurrenciesOnDateAsync(now, currenciesInfo, cancellationToken);

            return currenciesInfo.Single(currency => currency.Code == currencyType);
        }

        var currencyInfo = new CurrencyInfo
                           {
                               Code  = currencyType,
                               Value = newestInfo!.Currencies.Single(entity => entity.Code == currencyType).Value
                           };
        _logger.LogDebug("Found relevant info in cache {@Model}", currencyInfo);

        return currencyInfo;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyOnDateAsync(CurrencyType      currencyType,
                                                           DateOnly          date,
                                                           CancellationToken cancellationToken)
    {
        CurrenciesOnDateEntity? info = _repository.GetInfoOnDate(date);
        if (info is null)
        {
            _logger.LogDebug("Did not find relevant cache info for {Date}", date);

            CurrenciesOnDate currenciesOnDate =
                await _apiService.GetAllCurrenciesOnDateAsync(_settings.BaseCurrency, date, cancellationToken);

            await _repository.SaveCurrenciesOnDateAsync(currenciesOnDate.LastUpdatedAt,
                                                        currenciesOnDate.Currencies,
                                                        cancellationToken);

            return currenciesOnDate.Currencies.Single(currency => currency.Code == currencyType);
        }

        CurrencyInfoEntity relevantInfo = info.Currencies.Single(entity => entity.Code == currencyType);
        var currencyInfo = new CurrencyInfo
                           {
                               Code  = relevantInfo.Code,
                               Value = relevantInfo.Value
                           };
        _logger.LogDebug("Found relevant info at {Date} in cache {@Model}", relevantInfo.UpdatedAt, currencyInfo);

        return currencyInfo;
    }
}
