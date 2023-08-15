namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
/// Модель настроек API.
/// </summary>
public sealed record CurrenciesSettings
{
    /// <summary>Базовая валюта по умолчанию.</summary>
    public required string BaseCurrency { get; init; }

    /// <summary>Минимальный год для получения курса валют.</summary>
    public required int MinAvailableYear { get; init; }
}
