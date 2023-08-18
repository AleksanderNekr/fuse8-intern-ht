using Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

    internal Task<IEnumerable<FavoriteExchangeRateEntity>> GetAllFavoritesAsync(CancellationToken stopToken)
    {
        return Task.Run(() => _context.FavoriteExchangeRates.AsEnumerable(), stopToken);
    }

    internal ValueTask<FavoriteExchangeRateEntity?> GetFavoriteByNameAsync(
        string favoriteName, CancellationToken cancellationToken)
    {
        return _context.FavoriteExchangeRates.FindAsync(favoriteName, cancellationToken);
    }

    internal async Task<AddFavoriteResult> TryAddFavoriteAsync(string            name, CurrencyType currency,
                                                               CurrencyType      baseCurrency,
                                                               CancellationToken cancellationToken)
    {
        bool nameUnique = await CheckIfNameUniqueAsync(name, cancellationToken);
        if (!nameUnique)
        {
            return AddFavoriteResult.NameExists;
        }

        bool currenciesPairExists = await CurrenciesPairExists();
        if (currenciesPairExists)
        {
            return AddFavoriteResult.CurrencyPairExists;
        }

        FavoriteExchangeRateEntity favorite = new()
                                              {
                                                  Name         = name,
                                                  Currency     = currency,
                                                  BaseCurrency = baseCurrency
                                              };

        await _context.FavoriteExchangeRates.AddAsync(favorite, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return AddFavoriteResult.Success;

        async Task<bool> CurrenciesPairExists()
        {
            return await _context.FavoriteExchangeRates
                                 .AnyAsync(entity => entity.Currency == currency && entity.BaseCurrency == baseCurrency,
                                           cancellationToken);
        }
    }

    internal async Task<UpdateFavoriteResult> TryUpdateFavoriteAsync(string            oldName,
                                                                     string?           newName           = null,
                                                                     CurrencyType?     newCurrency       = null,
                                                                     CurrencyType?     newBaseCurrency   = null,
                                                                     CancellationToken cancellationToken = default)
    {
        FavoriteExchangeRateEntity? found = await _context.FavoriteExchangeRates.FindAsync(oldName, cancellationToken);
        if (found is null)
        {
            return UpdateFavoriteResult.NotFound;
        }

        if (newName is not null && newName != oldName)
        {
            bool unique = await CheckIfNameUniqueAsync(newName, cancellationToken);
            if (!unique)
            {
                return UpdateFavoriteResult.NameExists;
            }
        }

        newCurrency     ??= found.Currency;
        newBaseCurrency ??= found.BaseCurrency;

        bool pairExists = await CurrenciesPairExists();
        if (pairExists)
        {
            return UpdateFavoriteResult.CurrencyPairExists;
        }

        if (newName is not null)
        {
            found.Name = newName;
        }

        found.Currency     = newCurrency.Value;
        found.BaseCurrency = newBaseCurrency.Value;

        _context.Update(found);
        await _context.SaveChangesAsync(cancellationToken);

        return UpdateFavoriteResult.Success;

        async Task<bool> CurrenciesPairExists()
        {
            return await _context.FavoriteExchangeRates.AnyAsync(entity =>
                                                                     entity.Currency     == newCurrency
                                                                  && entity.BaseCurrency == newBaseCurrency
                                                                  && entity.Name         != oldName,
                                                                 cancellationToken);
        }
    }

    private async Task<bool> CheckIfNameUniqueAsync(string name, CancellationToken cancellationToken)
    {
        FavoriteExchangeRateEntity? existing = await _context.FavoriteExchangeRates.FindAsync(name, cancellationToken);

        return existing is null;
    }
}

public enum UpdateFavoriteResult
{
    Success,
    NotFound,
    NameExists,
    CurrencyPairExists
}

internal enum AddFavoriteResult
{
    Success,
    NameExists,
    CurrencyPairExists
}
