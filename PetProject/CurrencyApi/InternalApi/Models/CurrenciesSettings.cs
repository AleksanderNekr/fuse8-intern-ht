namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
/// Модель настроек API.
/// </summary>
/// <param name="BaseCurrency">Базовая валюта по умолчанию.</param>
/// <param name="CacheRelevanceHours">Время в часах – срок активности файла кэша.</param>
public record CurrenciesSettings(string BaseCurrency, int CacheRelevanceHours);
