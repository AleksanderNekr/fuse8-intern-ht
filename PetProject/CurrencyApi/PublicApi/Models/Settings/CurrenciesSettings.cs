namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;

public sealed record CurrenciesSettings
{
    public required string DefaultCurrency { get; init; }

    public required int DecimalPlace { get; init; }

    internal const int MinimumDecimalPlace = 0;

    internal const int MaximumDecimalPlace = 28;
}
