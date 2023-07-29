using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

/// <summary>
///     Модель информации о настройках.
/// </summary>
public readonly record struct SettingsInfo
{
    /// <summary>
    ///     Валюта по умолчанию.
    /// </summary>
    [JsonPropertyName("defaultCurrency")]
    public required string DefaultCurrency { get; init; }

    /// <summary>
    ///     Базовая валюта.
    /// </summary>
    [JsonPropertyName("baseCurrency")]
    public required string BaseCurrency { get; init; }

    /// <summary>
    ///     Лимит запросов.
    /// </summary>
    [JsonPropertyName("requestLimit")]
    public required int RequestLimit { get; init; }

    /// <summary>
    ///     Количество использованных запросов.
    /// </summary>
    [JsonPropertyName("requestCount")]
    public required int RequestCount { get; init; }

    /// <summary>
    ///     Количество знаков после запятой у валюты.
    /// </summary>
    [JsonPropertyName("currencyRoundCount")]
    public required int CurrencyRoundCount { get; init; }
}
