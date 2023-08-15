using Fuse8_ByteMinds.SummerSchool.Grpc;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

/// <inheritdoc />
public class CurrencyGrpcService : CurrencyApiGrpc.CurrencyApiGrpcBase
{
    private readonly ICachedCurrencyAPI           _cachedCurrencyApi;
    private readonly ICurrencyApiService          _currencyApiService;
    private readonly CurrenciesSettings           _settings;
    private readonly ILogger<CurrencyGrpcService> _logger;

    /// <inheritdoc />
    public CurrencyGrpcService(ICachedCurrencyAPI                  cachedCurrencyApi,
                               ICurrencyApiService                 currencyApiService,
                               IOptionsMonitor<CurrenciesSettings> optionsMonitor,
                               ILogger<CurrencyGrpcService>        logger)
    {
        _cachedCurrencyApi  = cachedCurrencyApi;
        _currencyApiService = currencyApiService;
        _settings           = optionsMonitor.CurrentValue;
        _logger             = logger;
    }

    /// <inheritdoc />
    public override async Task<CurrencyResponse> GetCurrentCurrency(CurrencyRequest request, ServerCallContext context)
    {
        CurrencyInfo currencyInfo = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
                                         (CurrencyType)request.Code,
                                         context.CancellationToken);
        _logger.LogDebug("Received model from API: {Model}", currencyInfo);

        CurrencyResponse response = new()
                                    {
                                        Value = currencyInfo.Value
                                    };

        return response;
    }

    /// <inheritdoc />
    public override async Task<CurrencyResponse> GetCurrencyOnDate(CurrencyOnDateRequest request,
                                                                   ServerCallContext     context)
    {
        CurrencyInfo currencyInfo = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
                                         (CurrencyType)request.Code,
                                         DateOnly.FromDateTime(request.Date.ToDateTime()),
                                         context.CancellationToken);
        _logger.LogDebug("Received model from API: {Model}", currencyInfo);

        CurrencyResponse response = new()
                                    {
                                        Value = currencyInfo.Value
                                    };

        return response;
    }

    /// <inheritdoc />
    public override async Task<SettingsResponse> GetSettings(Empty request, ServerCallContext context)
    {
        MonthSection monthSection = await _currencyApiService.GetMonthSectionAsync(context.CancellationToken);
        _logger.LogDebug("Received Month model from API: {Model}", monthSection);

        SettingsResponse settings = new()
                                    {
                                        BaseCurrency         = _settings.BaseCurrency,
                                        HasAvailableRequests = monthSection.Total > monthSection.Used
                                    };

        return settings;
    }
}
