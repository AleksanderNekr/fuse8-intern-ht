using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

/// <summary>
///     Модель информации о валюте.
/// </summary>
public readonly record struct CurrencyInfo
{
    /// <summary>
    ///     Код валюты.
    /// </summary>
    [JsonPropertyName("code")]
    public required CurrencyType Code { get; init; }

    /// <summary>
    ///     Значение валюты относительно базовой валюты.
    /// </summary>
    [JsonPropertyName("value")]
    public required decimal Value { get; init; }
}

/// <summary>
/// Тип валюты.
/// </summary>
public enum CurrencyType
{
    /// <summary>
    /// Доллар США.
    /// </summary>
    USD,
    /// <summary>
    /// Российский рубль.
    /// </summary>
    RUB,
    /// <summary>
    /// Казахстанский тенге.
    /// </summary>
    KZT,
    /// <summary>
    /// Евро
    /// </summary>
    EUR
}