using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
///     Модель информации о валюте.
/// </summary>
public readonly record struct CurrencyInfo
{
    /// <summary>
    ///     Код валюты.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    /// <summary>
    ///     Значение валюты относительно базовой валюты.
    /// </summary>
    [JsonPropertyName("value")]
    public required decimal Value { get; init; }
}
