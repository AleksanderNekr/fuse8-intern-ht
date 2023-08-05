using System.Net;
using System.Text.Json;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

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

        return new CurrencyInfo
               {
                   Code  = currency,
                   Value = value,
               };
    }

    /// <inheritdoc />
    public async Task<CurrencyOnDateInfo> GetCurrencyInfoOnDateAsync(string            currency,
                                                                     string            baseCurrency,
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
}
