using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Grpc;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

/// <inheritdoc />
public sealed class CurrencyApiService : ICurrencyApiService
{
    private readonly CurrencyApiGrpc.CurrencyApiGrpcClient _grpcClient;
    private readonly CurrenciesSettings                    _settings;

    public CurrencyApiService(CurrencyApiGrpc.CurrencyApiGrpcClient grpcClient,
                              IOptionsMonitor<CurrenciesSettings>   optionsMonitor)
    {
        _grpcClient = grpcClient;
        _settings   = optionsMonitor.CurrentValue;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyInfoAsync(CurrencyType      currency,
                                                         int               decimalPlace,
                                                         CancellationToken stopToken)
    {
        CurrencyRequest request = new() { Code = (CurrencyCode)currency };
        CurrencyResponse response = await _grpcClient.GetCurrentCurrencyAsync(request,
                                                                              options: new CallOptions(
                                                                                   cancellationToken: stopToken));
        decimal value   = decimal.Parse(response.Value);
        decimal rounded = Math.Round(value, decimalPlace);

        return new CurrencyInfo
               {
                   Code  = currency,
                   Value = rounded,
               };
    }

    /// <inheritdoc />
    public async Task<CurrencyOnDateInfo> GetCurrencyInfoOnDateAsync(CurrencyType      currency,
                                                                     int               decimalPlace,
                                                                     DateOnly          date,
                                                                     CancellationToken stopToken)
    {
        var dateTime = date.ToDateTime(TimeOnly.MaxValue);
        CurrencyOnDateRequest request = new()
                                        {
                                            Date = Timestamp.FromDateTime(dateTime),
                                            Code = (CurrencyCode)currency
                                        };
        CurrencyResponse response = await _grpcClient.GetCurrencyOnDateAsync(request,
                                                                             options: new CallOptions(
                                                                                  cancellationToken: stopToken));
        decimal value   = decimal.Parse(response.Value);
        decimal rounded = Math.Round(value, decimalPlace);

        return new CurrencyOnDateInfo
               {
                   Date  = date,
                   Code  = currency,
                   Value = rounded,
               };
    }

    /// <inheritdoc />
    public async Task<SettingsInfo> GetSettingsAsync(CancellationToken stopToken)
    {
        SettingsResponse response = await _grpcClient.GetSettingsAsync(new Empty(),
                                                                       new CallOptions(cancellationToken: stopToken));

        return new SettingsInfo
               {
                   DefaultCurrency      = _settings.DefaultCurrency,
                   BaseCurrency         = response.BaseCurrency,
                   NewRequestsAvailable = response.HasAvailableRequests,
                   CurrencyRoundCount   = _settings.DecimalPlace
               };
    }
}
