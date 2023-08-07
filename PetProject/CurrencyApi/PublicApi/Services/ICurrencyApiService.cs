using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

/// <summary>
///     Сервис получения данных от CurrencyApi.
/// </summary>
public interface ICurrencyApiService
{
    /// <summary>
    ///     Получение информации о валюте относительно базовой.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="decimalPlace">Количество знаков после запятой.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns> Объект типа <see cref="CurrencyInfo" />. </returns>
    public Task<CurrencyInfo> GetCurrencyInfoAsync(CurrencyType      currency,
                                                   int               decimalPlace,
                                                   CancellationToken stopToken);

    /// <summary>
    ///     Получение информации о валюте относительно базовой на определенную дату.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="decimalPlace">Количество знаков после запятой.</param>
    /// <param name="date">Дата, на которую получена информация.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>Объект типа <see cref="CurrencyOnDateInfo" />.</returns>
    public Task<CurrencyOnDateInfo> GetCurrencyInfoOnDateAsync(CurrencyType      currency,
                                                               int               decimalPlace,
                                                               DateOnly          date,
                                                               CancellationToken stopToken);

    /// <summary>
    /// Получение информации о текущих настройках приложения.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>Модель настроек.</returns>
    public Task<SettingsInfo> GetSettingsAsync(CancellationToken stopToken);
}
