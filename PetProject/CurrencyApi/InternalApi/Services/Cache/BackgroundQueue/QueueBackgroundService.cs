using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.BackgroundQueue;

/// <inheritdoc />
public sealed class QueueBackgroundService : BackgroundService
{
    private readonly IBackgroundTaskQueue            _taskQueue;
    private readonly ILogger<QueueBackgroundService> _logger;
    private readonly IServiceProvider                _serviceProvider;

    /// <inheritdoc />
    public QueueBackgroundService(IBackgroundTaskQueue            taskQueue,
                                  ILogger<QueueBackgroundService> logger,
                                  IServiceProvider                serviceProvider)
    {
        _taskQueue       = taskQueue;
        _logger          = logger;
        _serviceProvider = serviceProvider;
        _logger.LogDebug("Service initialized");
    }

    /// <inheritdoc />
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Service started");
        await ProcessTasksAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    private async Task ProcessTasksAsync(CancellationToken stopToken)
    {
        try
        {
            using IServiceScope         scope      = _serviceProvider.CreateScope();
            var                         context    = scope.ServiceProvider.GetRequiredService<CurrencyInternalContext>();
            IQueryable<CacheTaskEntity> tasks      = PendingTasks(context);
            int                         tasksCount = await tasks.CountAsync(stopToken);
            _logger.LogDebug("Found {Count} pending tasks", tasksCount);
            if (tasksCount > 1)
            {
                IOrderedQueryable<CacheTaskEntity> orderedByDate = tasks.OrderBy(static x => x.AddedAt);
                await EnqueueLastTask(orderedByDate);
                await SetTasksBeforeLastCanceled(orderedByDate);
            }
            else if (tasksCount == 1)
            {
                CacheTaskEntity enqueuingTask = await tasks.SingleAsync(stopToken);
                await EnqueueTask(enqueuingTask);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while trying to process tasks");

            throw;
        }

        return;

        static IQueryable<CacheTaskEntity> PendingTasks(CurrencyInternalContext context)
        {
            return context.CacheTasks
                          .Where(static x => x.Status == Status.Created || x.Status == Status.Running);
        }

        async Task EnqueueLastTask(IOrderedQueryable<CacheTaskEntity> orderedTasks)
        {
            CacheTaskEntity enqueuingTask = await orderedTasks.LastAsync(stopToken);
            AddTaskResult   result        = await _taskQueue.EnqueueAsync(enqueuingTask, stopToken);
            ThrowIfBadResult(result, enqueuingTask);
            _logger.LogDebug("Enqueued last task {Task}", enqueuingTask);
        }

        async Task SetTasksBeforeLastCanceled(IOrderedQueryable<CacheTaskEntity> orderedByDate)
        {
            await orderedByDate.SkipLast(1)
                               .ForEachAsync(static x => x.Status = Status.Canceled, stopToken);
        }

        async Task EnqueueTask(CacheTaskEntity task)
        {
            AddTaskResult result = await _taskQueue.EnqueueAsync(task, stopToken);
            ThrowIfBadResult(result, task);
            _logger.LogDebug("Enqueued task {Task}", task);
        }

        static void ThrowIfBadResult(AddTaskResult result, CacheTaskEntity enqueuingTask)
        {
            if (result != AddTaskResult.Success)
            {
                throw new InvalidOperationException($"Error while trying to enqueue task {enqueuingTask}"
                                                  + $"\nAdd result: {result}");
            }
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Service executed");
        while (!stoppingToken.IsCancellationRequested)
        {
            CacheTaskEntity task = await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                using IServiceScope scope  = _serviceProvider.CreateScope();
                var                 worker = scope.ServiceProvider.GetRequiredService<IRecalculationWorker>();
                await worker.RecalculateCacheAsync(task.Id, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while trying to run task {Task}", task);
            }
        }
    }
}
