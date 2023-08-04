namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

/// <summary>
///     Курсы валют на конкретную дату
/// </summary>
/// <param name="LastUpdatedAt">Дата обновления данных</param>
/// <param name="Currencies">Список курсов валют</param>
public readonly record struct CurrenciesOnDate(DateTime LastUpdatedAt, CurrencyInfo[] Currencies);
