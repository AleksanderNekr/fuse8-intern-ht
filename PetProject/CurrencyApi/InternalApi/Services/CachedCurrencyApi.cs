using System.Collections.Immutable;
using System.Text.Json;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public class CachedCurrencyApi : ICachedCurrencyAPI
{
    private readonly ICurrencyApiService          _currencyApi;
    private readonly CurrenciesSettings           _settings;
    private          DirectoryInfo?               _cacheDirInfo;
    private          ImmutableSortedSet<FileInfo> _cacheFilesInfo = null!;

    private static readonly string CacheFolderPath = Path.Combine(Directory.GetCurrentDirectory(),
                                                                  CacheConstants.CacheFolderName);


    public CachedCurrencyApi(ICurrencyApiService currencyApi, IOptionsMonitor<CurrenciesSettings> currenciesMonitor)
    {
        _currencyApi = currencyApi;
        _settings    = currenciesMonitor.CurrentValue;
    }

    public async Task<CurrencyInfo> GetCurrentCurrencyAsync(string            currencyType,
                                                            CancellationToken cancellationToken)
    {
        UpdateCacheInfo();
        var hourDifference = int.MaxValue;
        if (_cacheFilesInfo.Count > 0)
        {
            hourDifference = GetHourDifferenceWithCache();
        }

        CurrencyInfo currencyInfo;
        if (_cacheFilesInfo.Count == 0 || hourDifference > _settings.CacheRelevanceHours)
        {
            currencyInfo = await _currencyApi.GetCurrencyInfoAsync(currencyType,
                                                                   _settings.BaseCurrency,
                                                                   cancellationToken);
            await SaveToCache(currencyInfo, cancellationToken);

            return currencyInfo;
        }

        FileInfo newestFile = _cacheFilesInfo[0];
        currencyInfo = await GetFromCache(newestFile, cancellationToken);

        return currencyInfo;
    }

    public async Task<CurrencyInfo> GetCurrencyOnDateAsync(string            currencyType,
                                                           DateOnly          date,
                                                           CancellationToken cancellationToken)
    {
        UpdateCacheInfo();
        FileInfo? relevantFile = TryGetRelevantFile();

        CurrencyInfo currencyInfo;
        if (relevantFile is null)
        {
            currencyInfo = await _currencyApi.GetCurrencyInfoAsync(currencyType,
                                                                   _settings.BaseCurrency,
                                                                   date,
                                                                   cancellationToken);
            await SaveToCache(currencyInfo, cancellationToken);

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

    private static async Task SaveToCache(CurrencyInfo currencyInfo, CancellationToken cancellationToken)
    {
        await using FileStream fileStream = new(CacheFolderPath, FileMode.CreateNew);
        await JsonSerializer.SerializeAsync(fileStream, currencyInfo, cancellationToken: cancellationToken);
    }

    private static async Task<CurrencyInfo> GetFromCache(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        await using FileStream readFileStream = fileInfo.OpenRead();
        var currency = await JsonSerializer.DeserializeAsync<CurrencyInfo>(readFileStream,
                                                                           cancellationToken: cancellationToken);

        return currency;
    }

    private int GetHourDifferenceWithCache()
    {
        DateTime current       = DateTime.Now;
        DateTime newestInCache = DateTime.Parse(_cacheFilesInfo[0].Name);

        int hourDifference = (current - newestInCache).Hours;

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
