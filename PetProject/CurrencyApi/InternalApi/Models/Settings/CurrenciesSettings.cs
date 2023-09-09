using System.Diagnostics.CodeAnalysis;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;

/// <summary>
/// Модель настроек API.
/// </summary>
public sealed record CurrenciesSettings
{
    private string _baseCurrency;

    /// <summary>Базовая валюта по умолчанию.</summary>
    public required string BaseCurrency
    {
        get => _baseCurrency;
        [MemberNotNull(nameof(_baseCurrency))]
        set => Interlocked.Exchange(ref _baseCurrency, value);
    }

    /// <summary>Минимальный год для получения курса валют.</summary>
    public required int MinAvailableYear { get; init; }
}
