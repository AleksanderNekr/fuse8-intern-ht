using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data;

public sealed class CurrencyPublicRepository
{
    private readonly CurrencyPublicContext _context;

    private const int ExpectedSettingsRowsChanged = 1;

    public CurrencyPublicRepository(CurrencyPublicContext context)
    {
        _context = context;
    }

    internal async Task<CurrenciesSettings> GetSettingsAsync(CancellationToken stopToken)
    {
        CurrenciesSettings settings = await _context.Settings.SingleAsync(cancellationToken: stopToken);

        return settings;
    }

    internal async Task<bool> TryUpdateDefaultCurrencyAsync(CurrencyType newCurrency, CancellationToken stopToken)
    {
        int rowsChanged = await _context.Settings.ExecuteUpdateAsync(calls => calls.SetProperty(
                                                                          static settings => settings.DefaultCurrency,
                                                                          newCurrency.ToString()),
                                                                     stopToken);

        return rowsChanged == ExpectedSettingsRowsChanged;
    }

    internal async Task<bool> TryUpdateCurrencyRoundCountAsync(int newDecimalPlace, CancellationToken stopToken)
    {
        int rowsChanged = await _context.Settings.ExecuteUpdateAsync(calls => calls.SetProperty(
                                                                          static settings => settings.DecimalPlace,
                                                                          newDecimalPlace),
                                                                     stopToken);

        return rowsChanged == ExpectedSettingsRowsChanged;
    }
}
