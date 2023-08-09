using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Microsoft.Extensions.Options;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public sealed class CacheWorkerService
{
    private readonly ILogger<CacheWorkerService>   _logger;
    private          DirectoryInfo?                _cacheDirInfo;
    private          ImmutableSortedSet<FileInfo>? _cacheFilesInfo;

    private readonly DateTimeFormatInfo _dateTimeFormat;
    private readonly string             _filesSearchPattern;
    private readonly string             _cacheFolderPath;
    private readonly string             _fileExtension;

    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            Converters    = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

    public CacheWorkerService(ILogger<CacheWorkerService> logger, IOptionsMonitor<CacheSettings> optionsMonitor)
    {
        _logger = logger;
        CacheSettings settings = optionsMonitor.CurrentValue;
        _fileExtension      = settings.FileExtension;
        _filesSearchPattern = $"*{_fileExtension}";
        _dateTimeFormat = new DateTimeFormatInfo
                          {
                              ShortDatePattern    = settings.DatePattern,
                              LongDatePattern     = settings.DatePattern,
                              ShortTimePattern    = settings.TimePattern,
                              LongTimePattern     = settings.TimePattern,
                              FullDateTimePattern = $"{settings.DatePattern} {settings.TimePattern}",
                              DateSeparator       = settings.DateSeparator,
                              TimeSeparator       = settings.TimeSeparator
                          };
        _cacheFolderPath = Path.Combine(Directory.GetCurrentDirectory(), settings.CacheFolderName);
    }

    internal async Task SaveToCache(DateTime          updatedAt,
                                    CurrencyInfo[]    currenciesInfo,
                                    CancellationToken cancellationToken)
    {
        _logger.LogDebug("Before saving. Last write time: {WriteTime}", _cacheDirInfo?.LastWriteTime);
        string fileName = DateTimeToFileName(updatedAt);
        string filePath = Path.Combine(_cacheFolderPath, fileName);

        await using FileStream fileStream = new(filePath, FileMode.CreateNew);
        await JsonSerializer.SerializeAsync(fileStream,
                                            currenciesInfo,
                                            cancellationToken: cancellationToken,
                                            options: JsonSerializerOptions);

        _logger.LogDebug("Saved to cache {Name} {Currency}. Last write time: {WriteTime}",
                         fileName,
                         currenciesInfo,
                         _cacheDirInfo?.LastWriteTime);
    }

    internal async Task<CurrencyInfo[]> GetFromCache(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Before receiving. Last write time: {WriteTime}", _cacheDirInfo?.LastWriteTime);
        await using FileStream readFileStream = fileInfo.OpenRead();
        CurrencyInfo[] currencies = await JsonSerializer.DeserializeAsync<CurrencyInfo[]>(readFileStream,
                                        cancellationToken: cancellationToken,
                                        options: JsonSerializerOptions)
                                 ?? throw new InvalidOperationException("Cannot get data from cache file!");
        _logger.LogDebug("Received from cache file {Name} {Content}. Last write time: {WriteTime}",
                         fileInfo.Name,
                         currencies,
                         _cacheDirInfo?.LastWriteTime);

        return currencies;
    }

    internal void UpdateCacheInfo()
    {
        _logger.LogDebug("Before updating. Last write time: {WriteTime}", _cacheDirInfo?.LastWriteTime);
        var cacheDirInfo = new DirectoryInfo(_cacheFolderPath);

        if (_cacheDirInfo is null || _cacheFilesInfo is null || Changed())
        {
            _logger.LogDebug("Cache updating. Last write time: {WriteTime}", _cacheDirInfo?.LastWriteTime);
            _cacheDirInfo = cacheDirInfo;
            _cacheFilesInfo = cacheDirInfo.EnumerateFiles(_filesSearchPattern)
                                          .ToImmutableSortedSet(comparer: Comparer<FileInfo>.Create(Compare));
            _logger.LogDebug("Cache updated. Last write time: {WriteTime}", _cacheDirInfo.LastWriteTime);

            return;
        }

        _logger.LogDebug("Cache did not change. Last write time: {WriteTime}", _cacheDirInfo.LastWriteTime);

        return;

        bool Changed()
        {
            return cacheDirInfo.LastWriteTime != _cacheDirInfo.LastWriteTime;
        }
    }

    internal FileInfo? TryGetNewestFile()
    {
        return CacheEmpty()
                   ? null
                   : _cacheFilesInfo?[0];
    }

    internal bool CacheIsOlderThan(int hours)
    {
        return GetHourDifferenceWithNewest() > hours;
    }

    internal FileInfo? TryGetFileOnDate(DateOnly date)
    {
        return _cacheFilesInfo?.FirstOrDefault(file =>
                                               {
                                                   DateTime dateTimeCreation = ParseDateTimeFromFileName(file);
                                                   DateOnly dateCreation     = DateOnly.FromDateTime(dateTimeCreation);

                                                   return dateCreation == date;
                                               });
    }

    private double? GetHourDifferenceWithNewest()
    {
        FileInfo? newestFile = TryGetNewestFile();
        if (newestFile is null)
        {
            return null;
        }

        DateTime current = DateTime.Now;
        DateTime another = ParseDateTimeFromFileName(newestFile);

        double hourDifference = (current - another).TotalHours;

        return hourDifference;
    }

    private DateTime ParseDateTimeFromFileName(FileSystemInfo file)
    {
        return DateTime.Parse(Path.GetFileNameWithoutExtension(file.Name), _dateTimeFormat);
    }

    private string DateTimeToFileName(DateTime dateTime)
    {
        return $"{dateTime.ToString(_dateTimeFormat)}{_fileExtension}";
    }

    private bool CacheEmpty()
    {
        return _cacheFilesInfo is null || _cacheFilesInfo.Count == 0;
    }

    private int Compare(FileInfo? x, FileInfo? y)
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
        return ParseDateTimeFromFileName(y).CompareTo(ParseDateTimeFromFileName(x));
    }
}
