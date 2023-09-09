using System.Threading.Channels;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.BackgroundQueue;

/// <inheritdoc />
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private const    int                      MaxTasksAmount = 100;
    private readonly Channel<CacheTaskEntity> _queue;

    /// <inheritdoc cref="IBackgroundTaskQueue"/>
    public BackgroundTaskQueue()
    {
        BoundedChannelOptions channelOptions = new(capacity: MaxTasksAmount)
                                               {
                                                   FullMode = BoundedChannelFullMode.Wait
                                               };
        _queue = Channel.CreateBounded<CacheTaskEntity>(channelOptions);
    }

    /// <inheritdoc />
    public async ValueTask<AddTaskResult> EnqueueAsync(CacheTaskEntity cacheTask, CancellationToken stopToken)
    {
        if (!Enum.IsDefined(cacheTask.NewBaseCurrency))
        {
            return AddTaskResult.NewCurrencyUnknown;
        }

        await _queue.Writer.WriteAsync(cacheTask, stopToken);

        return AddTaskResult.Success;
    }

    /// <inheritdoc />
    public ValueTask<CacheTaskEntity> DequeueAsync(CancellationToken stopToken)
    {
        return _queue.Reader.ReadAsync(stopToken);
    }
}
