using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

/// <summary>
///     Модель секции данных о запросах на текущий месяц.
/// </summary>
public readonly record struct MonthSection
{
    /// <summary>
    ///     Всего запросов.
    /// </summary>
    [JsonPropertyName("total")]
    public required int Total { get; init; }

    /// <summary>
    ///     Использовано запросов.
    /// </summary>
    [JsonPropertyName("used")]
    public required int Used { get; init; }

    /// <summary>
    ///     Осталось запросов.
    /// </summary>
    [JsonPropertyName("remaining")]
    public required int Remaining { get; init; }
}
