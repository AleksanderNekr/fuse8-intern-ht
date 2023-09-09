namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;

/// <summary>
/// Возникает при попытке обновления данных кэша при наличии незавершенных задач на его обновление.
/// </summary>
internal sealed class CacheUpdateConcurrencyException : Exception
{
    public CacheUpdateConcurrencyException(string? msg) : base(msg)
    {
    }
}
