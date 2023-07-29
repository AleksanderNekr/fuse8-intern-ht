using System.Net;
using System.Text.Json;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

/// <inheritdoc />
public sealed class CurrencyApiService : ICurrencyApiService
{
    private readonly HttpClient _httpClient;

    public CurrencyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyInfoAsync(string            currency,
                                                         string            baseCurrency,
                                                         int               decimalPlace,
                                                         CancellationToken stopToken)
    {
        await CheckRequestsLimitAsync(stopToken);

        var requestUri = $"latest?currencies={currency}&base_currency={baseCurrency}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, stopToken);
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            throw new CurrencyNotFoundException(nameof(currency), currency);
        }

        response.EnsureSuccessStatusCode();

        string  responseBody = await response.Content.ReadAsStringAsync(stopToken);
        decimal value        = GetCurrencyFromResponse(currency, responseBody);
        decimal roundedValue = Math.Round(value, decimalPlace);

        return new CurrencyInfo
               {
                   Code  = currency,
                   Value = roundedValue,
               };
    }

    /// <inheritdoc />
    public async Task<CurrencyOnDateInfo> GetCurrencyInfoOnDateAsync(string            currency,
                                                                     string            baseCurrency,
                                                                     int               decimalPlace,
                                                                     DateOnly          date,
                                                                     CancellationToken stopToken)
    {
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
        decimal roundedValue = Math.Round(value, decimalPlace);

        return new CurrencyOnDateInfo
               {
                   Date  = date,
                   Code  = currency,
                   Value = roundedValue,
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
                   Total     = monthSection.GetProperty("limit").GetInt32(),
                   Remaining = monthSection.GetProperty("remaining").GetInt32(),
                   Used      = monthSection.GetProperty("used").GetInt32()
               };
    }

    /// <inheritdoc />
    public async Task<bool> IsConnectedAsync(CancellationToken stopToken)
    {
        const string        requestUri = "status";
        HttpResponseMessage response   = await _httpClient.GetAsync(requestUri, stopToken);

        return response.IsSuccessStatusCode;
    }

    private static decimal GetCurrencyFromResponse(string currency, string responseBody)
    {
        JsonDocument responseParsed  = JsonDocument.Parse(responseBody);
        JsonElement  dataSection     = responseParsed.RootElement.GetProperty("data");
        JsonElement  currencySection = dataSection.GetProperty(currency);
        decimal      value           = currencySection.GetProperty("value").GetDecimal();

        return value;
    }

    private async Task CheckRequestsLimitAsync(CancellationToken stopToken)
    {
        MonthSection monthSection = await GetMonthSectionAsync(stopToken);

        if (monthSection.Remaining > 0)
        {
            return;
        }

        throw new ApiRequestLimitException("API requests limit exceeded!");
    }
}
