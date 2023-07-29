using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public class MonthSection
{
    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("used")]
    public int Used { get; init; }

    [JsonPropertyName("remaining")]
    public int Remaining { get; init; }
}
