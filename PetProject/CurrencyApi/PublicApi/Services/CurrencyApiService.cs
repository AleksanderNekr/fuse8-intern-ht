using Fuse8_ByteMinds.SummerSchool.Grpc;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Settings;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

/// <inheritdoc />
public sealed class CurrencyApiService : ICurrencyApiService
{
    private readonly CurrencyApiGrpc.CurrencyApiGrpcClient _grpcClient;
    private readonly CurrencyPublicContext                 _context;

    public CurrencyApiService(CurrencyApiGrpc.CurrencyApiGrpcClient grpcClient,
                              CurrencyPublicContext                 context)
    {
        _grpcClient = grpcClient;
        _context    = context;
    }

    /// <inheritdoc />
    public async Task<CurrencyInfo> GetCurrencyInfoAsync(CurrencyType      currency,
                                                         int               decimalPlace,
                                                         CancellationToken stopToken)
    {
        CurrencyRequest request = new() { Code = (CurrencyCode)currency };
        CurrencyResponse response = await _grpcClient.GetCurrentCurrencyAsync(request,
                                                                              cancellationToken: stopToken);
        decimal value   = response.Value;
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
        var dateTime = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        CurrencyOnDateRequest request = new()
                                        {
                                            Date = Timestamp.FromDateTime(dateTime),
                                            Code = (CurrencyCode)currency
                                        };
        CurrencyResponse response = await _grpcClient.GetCurrencyOnDateAsync(request,
                                                                             cancellationToken: stopToken);
        decimal value   = response.Value;
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
                                                                       cancellationToken: stopToken);

        CurrenciesSettings settings = await _context.Settings.SingleAsync(cancellationToken: stopToken);

        return new SettingsInfo
               {
                   DefaultCurrency      = settings.DefaultCurrency,
                   BaseCurrency         = response.BaseCurrency,
                   NewRequestsAvailable = response.HasAvailableRequests,
                   CurrencyRoundCount   = settings.DecimalPlace
               };
    }
}
