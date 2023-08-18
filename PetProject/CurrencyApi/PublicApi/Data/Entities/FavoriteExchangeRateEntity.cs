using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Data.Entities;

public sealed class FavoriteExchangeRateEntity
{
    public string Name { get; set; } = null!;

    public CurrencyType Currency { get; set; }

    public CurrencyType BaseCurrency { get; set; }
}
