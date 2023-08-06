namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
/// Модель настроек API.
/// </summary>
public sealed record CurrenciesSettings
{
    /// <summary>Базовая валюта по умолчанию.</summary>
    public required string BaseCurrency { get; init; }

    /// <summary>Время в часах – срок активности файла кэша.</summary>
    public required int CacheRelevanceHours { get; init; }
}
