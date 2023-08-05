using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;

internal interface ICachedCurrencyAPI
{
    /// <summary>
    ///     Получает текущий курс
    /// </summary>
    /// <param name="currencyType">Валюта, для которой необходимо получить курс</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Текущий курс</returns>
    Task<CurrencyInfo> GetCurrentCurrencyAsync(string currencyType, CancellationToken cancellationToken);

    /// <summary>
    ///     Получает курс валюты, актуальный на <paramref name="date" />
    /// </summary>
    /// <param name="currencyType">Валюта, для которой необходимо получить курс</param>
    /// <param name="date">Дата, на которую нужно получить курс валют</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Курс на дату</returns>
    Task<CurrencyInfo> GetCurrencyOnDateAsync(string            currencyType,
                                              DateOnly          date,
                                              CancellationToken cancellationToken);
}
