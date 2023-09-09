namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.BackgroundQueue;

/// <summary>
/// Сервис для пересчета кэша.
/// </summary>
public interface IRecalculationWorker
{
    /// <summary>
    /// Выаполняет пересчет относительно задачи.
    /// </summary>
    /// <param name="taskId">ID задачи для пересчета.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    public Task RecalculateCacheAsync(Guid taskId, CancellationToken stopToken);
}
