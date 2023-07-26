using System.Net;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Newtonsoft.Json.Linq;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Extensions;

internal static class CurrencyApiExtensions
{
    public static async ValueTask<int> GetRemainingAsync(this HttpClient httpClient, CancellationToken stopToken)
    {
        dynamic monthSection = await GetMonthSectionAsync(httpClient, stopToken);

        return monthSection.remaining;
    }

    public static async ValueTask<int> GetLimitAsync(this HttpClient httpClient, CancellationToken stopToken)
    {
        dynamic monthSection = await GetMonthSectionAsync(httpClient, stopToken);

        return monthSection.total;
    }

    public static async ValueTask<int> GetUsedAsync(this HttpClient httpClient, CancellationToken stopToken)
    {
        dynamic monthSection = await GetMonthSectionAsync(httpClient, stopToken);

        return monthSection.used;
    }

    private static async Task<dynamic> GetMonthSectionAsync(HttpClient httpClient, CancellationToken stopToken)
    {
        const string        requestUri = CurrencyApiConstants.ApiStatusRequest;
        HttpResponseMessage response   = await httpClient.GetAsync(requestUri, stopToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new BadHttpRequestException(response.Headers.ToString());
        }

        string  responseBody   = await response.Content.ReadAsStringAsync(stopToken);
        dynamic responseParsed = JObject.Parse(responseBody);
        dynamic quotasSection  = responseParsed.quotas;

        return quotasSection.month;
    }

    public static async Task<decimal> GetCurrencyValue(this HttpClient   httpClient,
                                                       string            defaultCurrency,
                                                       string            baseCurrency,
                                                       string?           date      = null,
                                                       CancellationToken stopToken = default)
    {
        string requestUri = date is null
                                ? $"latest?currencies={defaultCurrency}&base_currency={baseCurrency}"
                                : $"historical?currencies={defaultCurrency}&base_currency={baseCurrency}&date={date}";

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, stopToken);
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            throw new CurrencyNotFoundException(nameof(defaultCurrency), defaultCurrency);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new BadHttpRequestException(response.Headers.ToString());
        }

        string  responseBody    = await response.Content.ReadAsStringAsync(stopToken);
        dynamic responseParsed  = JObject.Parse(responseBody);
        JObject dataSection     = responseParsed.data;
        var     currencySection = dataSection.Value<dynamic>(defaultCurrency)!;

        return currencySection.value;
    }
}
