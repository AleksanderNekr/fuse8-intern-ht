namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Settings;

public sealed class CurrenciesSettings
{
    public string DefaultCurrency { get; set; } = null!;

    public string BaseCurrency { get; set; } = null!;

    public int DecimalPlace { get; set; }
}
