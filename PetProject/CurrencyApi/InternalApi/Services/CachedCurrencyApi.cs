using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

/// <inheritdoc />
public class CachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly ICurrencyApiService          _currencyApi;
    private readonly CurrenciesSettings           _settings;
    private          DirectoryInfo?               _cacheDirInfo;
    private          ImmutableSortedSet<FileInfo> _cacheFilesInfo = null!;
    private readonly ICurrencyAPI               _currencyApi;
    private readonly CurrenciesSettings         _settings;
    private DirectoryInfo?               _cacheDirInfo;
    private ImmutableSortedSet<FileInfo> _cacheFilesInfo = null!;

    private static readonly IFormatProvider DateTimeCulture = CultureInfo.InvariantCulture;

    private static readonly string CacheFolderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                                                  CacheConstants.CacheFolderName);

    /// <inheritdoc cref="ICachedCurrencyAPI" />
    public CachedCurrencyApi(ICurrencyAPI                        currencyApi,
                             IOptionsMonitor<CurrenciesSettings> currenciesMonitor,
                             ILogger<CachedCurrencyApi>          logger)
    {
        _currencyApi = currencyApi;
        _settings    = currenciesMonitor.CurrentValue;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrentCurrencyAsync(CurrencyType      currencyType,
                                                            CancellationToken cancellationToken)
    {
        UpdateCacheInfo();
        var      hourDifference = int.MaxValue;
        FileInfo newestFile     = _cacheFilesInfo[0];
        if (_cacheFilesInfo.Count > 0)
        {
            hourDifference = GetHourDifferenceFromNow(newestFile);
        }

        CurrencyInfo currencyInfo;
        if (_cacheFilesInfo.Count == 0 || hourDifference > _settings.CacheRelevanceHours)
        {
            currencyInfo = await _currencyApi.GetCurrencyInfoAsync(currencyType,
                                                                   _settings.BaseCurrency,
                                                                   cancellationToken);
            await SaveToCache(currencyInfo, cancellationToken);
            CurrencyInfo[] currenciesInfo = await _currencyApi.GetAllCurrentCurrenciesAsync(
                                                 _settings.BaseCurrency,
                                                 cancellationToken);
            currencyInfo = currenciesInfo.Single(currency => currency.Code == currencyType);

            var fileName = $"{DateTime.Now.ToString(DateTimeCulture)}.json";
            await SaveToCache(fileName, currencyInfo, cancellationToken);

            return currencyInfo;
        }

        currencyInfo = await GetFromCache(newestFile, cancellationToken);

        return currencyInfo;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyOnDateAsync(CurrencyType      currencyType,
                                                           DateOnly          date,
                                                           CancellationToken cancellationToken)
    {
        UpdateCacheInfo();
        FileInfo? relevantFile = TryGetRelevantFile();

        CurrencyInfo currencyInfo;
        if (relevantFile is null)
        {
            _logger.LogDebug("Did not find relevant cache file");
            CurrenciesOnDate currenciesOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(
                                                     _settings.BaseCurrency,
                                                     date,
                                                     cancellationToken);
            currencyInfo = currenciesOnDate.Currencies.Single(currency => currency.Code == currencyType);

            var fileName = $"{currenciesOnDate.LastUpdatedAt.ToString(DateTimeCulture)}.json";
            await SaveToCache(fileName, currencyInfo, cancellationToken);

            return currencyInfo;
        }

        currencyInfo = await GetFromCache(relevantFile, cancellationToken);

        return currencyInfo;

        FileInfo? TryGetRelevantFile()
        {
            return _cacheFilesInfo.FirstOrDefault(file =>
                                                  {
                                                      DateTime dateTimeCreation = DateTime.Parse(file.Name);
                                                      DateOnly dateCreation     = DateOnly.FromDateTime(dateTimeCreation);

                                                      return dateCreation == date;
                                                  });
        }
    }

    private async Task SaveToCache(string fileName, CurrencyInfo currencyInfo, CancellationToken cancellationToken)
    {
        string filePath = Path.Combine(CacheFolderPath, fileName);

        await using FileStream fileStream = new(filePath, FileMode.CreateNew);
        await JsonSerializer.SerializeAsync(fileStream, currencyInfo, cancellationToken: cancellationToken);
    }

    private static async Task<CurrencyInfo> GetFromCache(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        await using FileStream readFileStream = fileInfo.OpenRead();
        var currency = await JsonSerializer.DeserializeAsync<CurrencyInfo>(readFileStream,
                                                                           cancellationToken: cancellationToken);

        return currency;
    }

    private static int GetHourDifferenceFromNow(FileSystemInfo file)
    {
        DateTime current = DateTime.Now;
        DateTime another = DateTime.Parse(file.Name);

        int hourDifference = (current - another).Hours;

        return hourDifference;
    }

    private void UpdateCacheInfo()
    {
        var cacheDirInfo = new DirectoryInfo(CacheFolderPath);
        if (_cacheDirInfo is not null && DidNotChange())
        {
            return;
        }

        _cacheDirInfo = cacheDirInfo;
        _cacheFilesInfo = _cacheDirInfo.EnumerateFiles(CacheConstants.FilesSearchPattern)
                                       .ToImmutableSortedSet(comparer: new FileInfoComparerByNameReversed());

        return;

        bool DidNotChange()
        {
            return cacheDirInfo.LastWriteTime.Equals(_cacheDirInfo.LastWriteTime);
        }
    }
}

internal sealed class FileInfoComparerByNameReversed : IComparer<FileInfo>
{
    public int Compare(FileInfo? x, FileInfo? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (ReferenceEquals(null, y))
        {
            return -1;
        }

        if (ReferenceEquals(null, x))
        {
            return 1;
        }

        // Дата создания файла может быть некорректной при передаче, поэтому сравнение не по ней, а по названию
        // файла, которое и есть дата его создания.
        return DateTime.Parse(y.Name).CompareTo(DateTime.Parse(x.Name));
    }
}
