using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;

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
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
                _logger.LogError(e, "Error while trying to process task {Task}", task);
            }
        }
    }
}
