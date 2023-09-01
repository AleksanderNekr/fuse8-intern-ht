#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;

public sealed class CacheTaskEntity
{
    public required Guid Id { get; init; }

    public required Status Status { get; init; }
}

public enum Status
{
    Created         = 0,
    Running         = 1,
    RanToCompletion = 2,
    Faulted         = 3,
    Canceled        = 4
}
