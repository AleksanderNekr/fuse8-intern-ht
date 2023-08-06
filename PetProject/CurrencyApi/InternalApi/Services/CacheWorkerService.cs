using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public sealed class CacheWorkerService
{
    private const string CacheFolderName    = "Cache";
    private const string FilesSearchPattern = "*.json";
    private const string DateSeparator      = "-";
    private const string TimeSeparator      = "_";

    private readonly ILogger<CacheWorkerService>  _logger;
    private          DirectoryInfo?               _cacheDirInfo;
    private          ImmutableSortedSet<FileInfo> _cacheFilesInfo = null!;

    private static readonly IFormatProvider DateTimeCulture =
        new DateTimeFormatInfo
        {
            DateSeparator = DateSeparator,
            TimeSeparator = TimeSeparator
        };

    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new()
        {
            Converters    = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

    private static readonly string CacheFolderPath = Path.Combine(Directory.GetCurrentDirectory(), CacheFolderName);

    public CacheWorkerService(ILogger<CacheWorkerService> logger)
    {
        _logger = logger;
    }

    internal async Task SaveToCache(DateTime          updatedAt,
                                    CurrencyInfo[]    currenciesInfo,
                                    CancellationToken cancellationToken)
    {
        string fileName = DateTimeToFileName(updatedAt);
        string filePath = Path.Combine(CacheFolderPath, fileName);

        await using FileStream fileStream = new(filePath, FileMode.CreateNew);
        await JsonSerializer.SerializeAsync(fileStream,
                                            currenciesInfo,
                                            cancellationToken: cancellationToken,
                                            options: JsonSerializerOptions);

        _logger.LogDebug("Saved to cache {Name}{Newline}{Currency}", fileName, Environment.NewLine, currenciesInfo);
    }

    internal async Task<CurrencyInfo[]> GetFromCache(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        await using FileStream readFileStream = fileInfo.OpenRead();
        CurrencyInfo[] currencies = await JsonSerializer.DeserializeAsync<CurrencyInfo[]>(readFileStream,
                                             cancellationToken: cancellationToken,
                                             options: JsonSerializerOptions)
                                 ?? throw new InvalidOperationException("Cannot get data from cache file!");
        _logger.LogDebug("Received from cache file {Name}{Newline}{Content}",
                         fileInfo.Name,
                         Environment.NewLine,
                         currencies);

        return currencies;
    }

    internal void UpdateCacheInfo()
    {
        var cacheDirInfo = new DirectoryInfo(CacheFolderPath);
        if (_cacheDirInfo is not null && DidNotChange())
        {
            _logger.LogDebug("Cache did not change");

            return;
        }

        _logger.LogDebug("Detected cache changes");
        _cacheDirInfo = cacheDirInfo;
        _cacheFilesInfo = _cacheDirInfo.EnumerateFiles(FilesSearchPattern)
                                       .ToImmutableSortedSet(comparer: new FileInfoComparerByNameReversed());

        return;

        bool DidNotChange()
        {
            return cacheDirInfo.LastWriteTime.Equals(_cacheDirInfo.LastWriteTime);
        }
    }

    internal FileInfo? TryGetNewestFile()
    {
        return CacheEmpty()
                   ? null
                   : _cacheFilesInfo[0];
    }

    internal bool CacheIsOlderThan(int hours)
    {
        return GetHourDifferenceWithNewest() > hours;
    }

    internal FileInfo? TryGetFileOnDate(DateOnly date)
    {
        return _cacheFilesInfo.FirstOrDefault(file =>
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

    private static DateTime ParseDateTimeFromFileName(FileSystemInfo file)
    {
        return DateTime.Parse(Path.GetFileNameWithoutExtension(file.Name), DateTimeCulture);
    }

    private static string DateTimeToFileName(DateTime dateTime)
    {
        return $"{dateTime.ToString(DateTimeCulture)}.json";
    }

    private bool CacheEmpty()
    {
        return _cacheFilesInfo.Count == 0;
    }

    private sealed class FileInfoComparerByNameReversed : IComparer<FileInfo>
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
            return ParseDateTimeFromFileName(y).CompareTo(ParseDateTimeFromFileName(x));
        }
    }
}
