using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data;

public sealed class CurrencyPublicRepository
{
    private readonly CurrencyPublicContext _context;

    public CurrencyPublicRepository(CurrencyPublicContext context)
    {
        _context = context;
    }

    internal async Task<CurrenciesSettings> GetSettingsAsync(CancellationToken stopToken)
    {
        CurrenciesSettings settings = await _context.Settings.SingleAsync(cancellationToken: stopToken);

        return settings;
    }
}
