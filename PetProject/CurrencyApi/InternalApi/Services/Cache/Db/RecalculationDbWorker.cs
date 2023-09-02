using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.BackgroundQueue;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Db;

/// <inheritdoc />
public sealed class RecalculationDbWorker : IRecalculationWorker
{
    private readonly CurrencyInternalContext        _context;
    private readonly CurrenciesSettings             _settings;
    private readonly ILogger<RecalculationDbWorker> _logger;

    /// <inheritdoc cref="IRecalculationWorker" />
    public RecalculationDbWorker(CurrencyInternalContext        context,
                                 CurrenciesSettings             settings,
                                 ILogger<RecalculationDbWorker> logger)
    {
        _context  = context;
        _settings = settings;
        _logger   = logger;
    }

    /// <inheritdoc />
    public async Task RecalculateCacheAsync(Guid taskId, CancellationToken stopToken)
    {
        CacheTaskEntity task = await _context.CacheTasks.FindAsync(taskId)
                            ?? throw new InvalidOperationException($"Task with provided id {taskId} not found");

        bool baseCurrenciesEqual = string.Equals(task.NewBaseCurrency.ToString(),
                                                 _settings.BaseCurrency,
                                                 StringComparison.OrdinalIgnoreCase);
        if (baseCurrenciesEqual)
        {
            _logger.LogDebug("Base currencies equal");
            await SetStatusAsync(Status.RanToCompletion);

            return;
        }

        await SetStatusAsync(Status.Running);

        IQueryable<IGrouping<DateTime, CurrencyInfoEntity>> currenciesByDate =
            _context.CurrencyInfos.GroupBy(static x => x.UpdatedAt);

        string oldBaseCurrency = _settings.BaseCurrency;
        try
        {
            await RecalculateAsync();
        }
        catch (Exception)
        {
            await _context.CurrencyInfos.ForEachAsync(x => _context.Entry(x).State = EntityState.Unchanged,
                                                      stopToken);
            await _context.Settings.UpdateBaseCurrencyAsync(oldBaseCurrency, _settings, stopToken);
            await SetStatusAsync(Status.Faulted);

            // Exception will be handled by middleware.
            throw;
        }

        return;

        async Task RecalculateAsync()
        {
            await currenciesByDate.ForEachAsync(group =>
                                                {
                                                    decimal denominator = group
                                                                         .Single(x => x.Code == task.NewBaseCurrency)
                                                                         .Value;
                                                    foreach (CurrencyInfoEntity currency in group)
                                                    {
                                                        currency.Value /= denominator;
                                                    }
                                                },
                                                stopToken);

            _context.UpdateRange(_context.CurrencyInfos);
            await _context.Settings.UpdateBaseCurrencyAsync(task.NewBaseCurrency.ToString(), _settings, stopToken);

            await SetStatusAsync(Status.RanToCompletion);
        }

        Task SetStatusAsync(Status newStatus)
        {
            if (task.Status == newStatus)
            {
                return Task.CompletedTask;
            }

            task.Status = newStatus;
            _context.Update(task);

            return _context.SaveChangesAsync(stopToken);
        }
    }
}
