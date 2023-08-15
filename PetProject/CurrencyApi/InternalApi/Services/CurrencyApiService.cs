using System.Net;
using System.Text.Json;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

/// <inheritdoc cref="Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts.ICurrencyApiService" />
public sealed class CurrencyApiService : ICurrencyApiService
{
    private readonly HttpClient         _httpClient;
    private readonly CurrenciesSettings _settings;
    private const    string             CurrenciesSeparator = ",";

    /// <inheritdoc cref="Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts.ICurrencyApiService" />
    public CurrencyApiService(HttpClient httpClient, IOptionsMonitor<CurrenciesSettings> optionsMonitor)
    {
        _httpClient = httpClient;
        _settings   = optionsMonitor.CurrentValue;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo[]> GetAllCurrentCurrenciesAsync(string            baseCurrency,
                                                                   CancellationToken cancellationToken)
    {
        await CheckRequestsLimitAsync(cancellationToken);

        CurrencyType[] currencies = Enum.GetValues<CurrencyType>();
        string         separated  = string.Join(CurrenciesSeparator, currencies);
        var            requestUri = $"latest?currencies={separated}&base_currency={baseCurrency}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureCurrencyFound();
        response.EnsureSuccessStatusCode();

        string                    responseBody   = await response.Content.ReadAsStringAsync(cancellationToken);
        IEnumerable<CurrencyInfo> currenciesInfo = GetCurrenciesInfoFromResponse(currencies, responseBody);

        return currenciesInfo.ToArray();
    }

    /// <inheritdoc />
    public async Task<CurrenciesOnDate> GetAllCurrenciesOnDateAsync(string            baseCurrency,
                                                                    DateOnly          date,
                                                                    CancellationToken cancellationToken)
    {
        EnsureDateIsCorrect(date);
        await CheckRequestsLimitAsync(cancellationToken);

        CurrencyType[] currencies = Enum.GetValues<CurrencyType>();
        string         separated  = string.Join(CurrenciesSeparator, currencies);
        var            requestUri = $"historical?currencies={separated}&base_currency={baseCurrency}&&date={date}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureCurrencyFound();
        response.EnsureSuccessStatusCode();

        string                    responseBody   = await response.Content.ReadAsStringAsync(cancellationToken);
        IEnumerable<CurrencyInfo> currenciesInfo = GetCurrenciesInfoFromResponse(currencies, responseBody);

        DateTime         updatedAt        = DateTimeFromResponse();
        CurrenciesOnDate currenciesOnDate = new(updatedAt, currenciesInfo.ToArray());

        return currenciesOnDate;

        DateTime DateTimeFromResponse()
        {
            JsonDocument responseParsed = JsonDocument.Parse(responseBody);
            JsonElement  metaSection    = responseParsed.RootElement.GetProperty("meta");
            DateTime     lastUpdatedAt  = metaSection.GetProperty("last_updated_at").GetDateTime();

            return lastUpdatedAt;
        }
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyInfoAsync(CurrencyType      currency,
                                                         string            baseCurrency,
                                                         CancellationToken stopToken)
    {
        await CheckRequestsLimitAsync(stopToken);

        var requestUri = $"latest?currencies={currency}&base_currency={baseCurrency}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, stopToken);
        response.EnsureCurrencyFound();
        response.EnsureSuccessStatusCode();

        string  responseBody = await response.Content.ReadAsStringAsync(stopToken);
        decimal value        = GetCurrencyFromResponse(currency, responseBody);

        return new CurrencyInfo
               {
                   Code  = currency,
                   Value = value,
               };
    }

    /// <inheritdoc />
    public async Task<CurrencyOnDateInfo> GetCurrencyInfoOnDateAsync(CurrencyType      currency,
                                                                     string            baseCurrency,
                                                                     DateOnly          date,
                                                                     CancellationToken stopToken)
    {
        EnsureDateIsCorrect(date);
        await CheckRequestsLimitAsync(stopToken);

        var requestUri = $"historical?currencies={currency}&base_currency={baseCurrency}&&date={date}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, stopToken);
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            throw new CurrencyNotFoundException(nameof(currency), currency);
        }

        response.EnsureSuccessStatusCode();

        string  responseBody = await response.Content.ReadAsStringAsync(stopToken);
        decimal value        = GetCurrencyFromResponse(currency, responseBody);

        return new CurrencyOnDateInfo
               {
                   Date  = date,
                   Code  = currency,
                   Value = value,
               };
    }

    /// <inheritdoc />
    public async Task<MonthSection> GetMonthSectionAsync(CancellationToken stopToken)
    {
        const string        requestUri = "status";
        HttpResponseMessage response   = await _httpClient.GetAsync(requestUri, stopToken);

        response.EnsureSuccessStatusCode();

        string       responseBody   = await response.Content.ReadAsStringAsync(stopToken);
        JsonDocument responseParsed = JsonDocument.Parse(responseBody);
        JsonElement  quotasSection  = responseParsed.RootElement.GetProperty("quotas");
        JsonElement  monthSection   = quotasSection.GetProperty("month");

        return new MonthSection
               {
                   Total     = monthSection.GetProperty("total").GetInt32(),
                   Remaining = monthSection.GetProperty("remaining").GetInt32(),
                   Used      = monthSection.GetProperty("used").GetInt32(),
               };
    }

    /// <inheritdoc />
    public async Task<bool> IsConnectedAsync(CancellationToken stopToken)
    {
        const string        requestUri = "status";
        HttpResponseMessage response   = await _httpClient.GetAsync(requestUri, stopToken);

        return response.IsSuccessStatusCode;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyInfoAsync(CurrencyType      currency,
                                                         string            baseCurrency,
                                                         DateOnly          date,
                                                         CancellationToken stopToken)
    {
        EnsureDateIsCorrect(date);
        await CheckRequestsLimitAsync(stopToken);

        var requestUri = $"historical?currencies={currency}&base_currency={baseCurrency}&&date={date}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, stopToken);
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            throw new CurrencyNotFoundException(nameof(currency), currency);
        }

        response.EnsureSuccessStatusCode();

        string  responseBody = await response.Content.ReadAsStringAsync(stopToken);
        decimal value        = GetCurrencyFromResponse(currency, responseBody);

        return new CurrencyInfo
               {
                   Code  = currency,
                   Value = value,
               };
    }

    private void EnsureDateIsCorrect(DateOnly date)
    {
        DateOnly maxDate = DateOnly.FromDateTime(DateTime.Now);
        if (date.Year >= _settings.MinAvailableYear && date <= maxDate)
        {
            return;
        }

        DateOnly minDate = new(_settings.MinAvailableYear, 1, 1);

        throw new IncorrectDateException(date, minDate, maxDate);
    }

    private async Task CheckRequestsLimitAsync(CancellationToken stopToken)
    {
        const string        requestUri = "status";
        HttpResponseMessage response   = await _httpClient.GetAsync(requestUri, stopToken);

        response.EnsureSuccessStatusCode();

        string       responseBody   = await response.Content.ReadAsStringAsync(stopToken);
        JsonDocument responseParsed = JsonDocument.Parse(responseBody);
        JsonElement  quotasSection  = responseParsed.RootElement.GetProperty("quotas");
        JsonElement  monthSection   = quotasSection.GetProperty("month");
        int          remaining      = monthSection.GetProperty("remaining").GetInt32();

        if (remaining > 0)
        {
            return;
        }

        throw new ApiRequestLimitException("API requests limit exceeded!");
    }

    private static decimal GetCurrencyFromResponse(CurrencyType currency, string responseBody)
    {
        JsonDocument responseParsed  = JsonDocument.Parse(responseBody);
        JsonElement  dataSection     = responseParsed.RootElement.GetProperty("data");
        JsonElement  currencySection = dataSection.GetProperty(currency.ToString());
        decimal      value           = currencySection.GetProperty("value").GetDecimal();

        return value;
    }

    private static IEnumerable<CurrencyInfo> GetCurrenciesInfoFromResponse(IEnumerable<CurrencyType> currencies,
                                                                           string                    responseBody)
    {
        return currencies.Select(currency => new CurrencyInfo
                                             {
                                                 Code  = currency,
                                                 Value = GetCurrencyFromResponse(currency, responseBody)
                                             });
    }
}

file static class Extensions
{
    public static void EnsureCurrencyFound(this HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            throw new CurrencyNotFoundException(response.Content.ToString());
        }
    }
}
