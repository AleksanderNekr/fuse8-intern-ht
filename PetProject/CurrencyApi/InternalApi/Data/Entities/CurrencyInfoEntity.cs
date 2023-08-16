using System.ComponentModel.DataAnnotations;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Data.Entities;

public class CurrencyInfoEntity
{
    public CurrencyType Code { get; set; }

    public decimal Value { get; set; }

    // Nav property.
    public DateTime UpdatedAt { get; set; }

    // Nav property.
    public CurrenciesOnDateEntity CurrenciesOnDate { get; set; } = null!;
}
