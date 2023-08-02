namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

public sealed record CurrenciesSettings
{
    public required string DefaultCurrency { get; init; }

    public required string BaseCurrency { get; init; }

    public required int DecimalPlace { get; init; }
}
