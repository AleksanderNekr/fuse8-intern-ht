using System.ComponentModel.DataAnnotations;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;

public class CurrenciesOnDateEntity
{
    public DateTime LastUpdatedAt { get; set; }

    // Nav property.
    public List<CurrencyInfoEntity> Currencies { get; set; } = new();
}
