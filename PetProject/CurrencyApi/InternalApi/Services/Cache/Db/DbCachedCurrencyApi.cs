using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Db;

/// <inheritdoc />
public sealed class DbCachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly ILogger<DbCachedCurrencyApi> _logger;
    private readonly ICurrencyApiService          _apiService;
    private readonly DbCacheRepository            _repository;
    private readonly CurrenciesSettings           _settings;

    private const int WaitForTasksSeconds = 10;

    /// <inheritdoc cref="ICachedCurrencyAPI" />
    public DbCachedCurrencyApi(ILogger<DbCachedCurrencyApi> logger,
                               ICurrencyApiService          apiService,
                               CurrenciesSettings           settings,
                               DbCacheRepository            repository)
    {
        _logger     = logger;
        _apiService = apiService;
        _repository = repository;
        _settings   = settings;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrentCurrencyAsync(CurrencyType      currencyType,
                                                            CancellationToken cancellationToken)
    {
        if (_repository.InfoOutdated(out CurrenciesOnDateEntity? newestInfo))
        {
            _logger.LogDebug("Did not find relevant cache info");
            await CheckIfCacheIsReadyAsync(cancellationToken);

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
            await CheckIfCacheIsReadyAsync(cancellationToken);

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

    private async Task CheckIfCacheIsReadyAsync(CancellationToken cancellationToken)
    {
        if (await _repository.AnyPendingTasksAsync())
        {
            _logger.LogWarning("Found pending tasks, waiting...");
            await Task.Delay(TimeSpan.FromSeconds(WaitForTasksSeconds), cancellationToken);

            if (await _repository.AnyPendingTasksAsync())
            {
                _logger.LogError("Still unfinished tasks");

                throw new CacheUpdateConcurrencyException("Can't update cache while there are unfinished tasks");
            }

            _logger.LogDebug("All tasks completed");
        }
    }
}
