using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.BackgroundQueue;

/// <summary>
///  Предоставляет методы для работы с очередью задач обновления кэша.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Добавление задачи в очередь.
    /// </summary>
    /// <param name="cacheTask">Добавляемая задача.</param>
    /// <param name="stopToken">Токен отмены выполения операции.</param>
    /// <returns>Результат операции добавления.</returns>
    ValueTask<AddTaskResult> EnqueueAsync(CacheTaskEntity cacheTask, CancellationToken stopToken);
    
    /// <summary>
    /// Извлечение первой в очереди задачи.
    /// </summary>
    /// <param name="stopToken">Токен отмены выполения операции.</param>
    /// <returns>Сущность извлекаемой задачи.</returns>
    ValueTask<CacheTaskEntity> DequeueAsync(CancellationToken stopToken);
}

/// <summary>
/// Результат добавления задачи в очередь.
/// </summary>
public enum AddTaskResult
{
    /// <summary>
    /// Задача успешно добавлена.
    /// </summary>
    Success,
    /// <summary>
    /// Новая базовая валюта неизвестна.
    /// </summary>
    NewCurrencyUnknown,
}