#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;

public record CacheSettings
{
    public required string CacheFolderName { get; init; }

    public required string FileExtension { get; init; }

    public required string DateSeparator { get; init; }

    public required string TimeSeparator { get; init; }

    public required string DatePattern { get; init; }

    public required string TimePattern { get; init; }

    public required int CacheRelevanceHours { get; init; }
}
