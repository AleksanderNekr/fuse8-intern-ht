using System.Net;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;

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

        string  responseBody   = await response.Content.ReadAsStringAsync(stopToken);
        JObject responseParsed = JObject.Parse(responseBody);
        var     quotasSection  = responseParsed.GetValue<JObject>("quotas");
        var     monthSection   = quotasSection.GetValue<JObject>("month").ToObject<MonthSection>();

        return monthSection;
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
        JObject responseParsed  = JObject.Parse(responseBody);
        var     dataSection     = responseParsed.GetValue<JObject>("data");
        var     currencySection = dataSection.GetValue<JObject>(currency);
        var     value           = currencySection.GetValue<decimal>("value");

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
