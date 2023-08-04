using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
///     Модель информации о валюте на определенную дату.
/// </summary>
public readonly record struct CurrencyOnDateInfo
{
    /// <summary>
    ///     Дата, на которую была получена информация.
    /// </summary>
    [JsonPropertyName("date")]
    public required DateOnly Date { get; init; }

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
