using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;

/// <summary>
///     Сервис получения данных от CurrencyApi.
/// </summary>
public interface ICurrencyApiService
{
    /// <summary>
    ///     Получение информации о валюте относительно базовой.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="baseCurrency">Базовая валюта.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns> Объект типа <see cref="CurrencyInfo" />. </returns>
    /// <exception cref="CurrencyNotFoundException">Превышен лимит запросов к API.</exception>
    /// <exception cref="HttpRequestException">Не найдена валюта.</exception>
    /// <exception cref="ApiRequestLimitException">HTTP-ответ был не успешен.</exception>
    public Task<CurrencyInfo> GetCurrencyInfoAsync(string            currency,
                                                   string            baseCurrency,
                                                   CancellationToken stopToken);

    /// <summary>
    ///     Получение информации о валюте относительно базовой.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="baseCurrency">Базовая валюта.</param>
    /// <param name="date">Дата, на которую получена информация.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns> Объект типа <see cref="CurrencyInfo" />. </returns>
    /// <exception cref="CurrencyNotFoundException">Превышен лимит запросов к API.</exception>
    /// <exception cref="HttpRequestException">Не найдена валюта.</exception>
    /// <exception cref="ApiRequestLimitException">HTTP-ответ был не успешен.</exception>
    public Task<CurrencyInfo> GetCurrencyInfoAsync(string            currency,
                                                   string            baseCurrency,
                                                   DateOnly          date,
                                                   CancellationToken stopToken);

    /// <summary>
    ///     Получение информации о валюте относительно базовой на определенную дату.
    /// </summary>
    /// <param name="currency">Валюта.</param>
    /// <param name="baseCurrency">Базовая валюта.</param>
    /// <param name="date">Дата, на которую получена информация.</param>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>Объект типа <see cref="CurrencyOnDateInfo" />.</returns>
    /// <exception cref="ApiRequestLimitException">Превышен лимит запросов к API.</exception>
    /// <exception cref="CurrencyNotFoundException">Не найдена валюта.</exception>
    /// <exception cref="HttpRequestException">HTTP-ответ был не успешен.</exception>
    public Task<CurrencyOnDateInfo> GetCurrencyInfoOnDateAsync(string            currency,
                                                               string            baseCurrency,
                                                               DateOnly          date,
                                                               CancellationToken stopToken);

    /// <summary>
    ///     Получении секции месяца в информации об использованных запросах к внешнему API.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>
    ///     Модель секции месяца <see cref="MonthSection" />.
    /// </returns>
    /// <exception cref="HttpRequestException">HTTP-ответ был не успешен.</exception>
    public Task<MonthSection> GetMonthSectionAsync(CancellationToken stopToken);

    /// <summary>
    ///     Проверяет, доступен ли сервер API.
    /// </summary>
    /// <param name="stopToken">Токен отмены операции.</param>
    /// <returns>true – удалось подключиться к серверу, false – не удалось.</returns>
    Task<bool> IsConnectedAsync(CancellationToken stopToken);
}
