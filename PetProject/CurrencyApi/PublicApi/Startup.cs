using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Audit.Serilog.Configuration;
using Fuse8_ByteMinds.SummerSchool.Grpc;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Filters;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration)
                                              .CreateLogger();

        services.AddLogging(static builder =>
                            {
                                builder.ClearProviders();
                                builder.AddSerilog(dispose: true);
                            });

        Configuration.Setup().UseSerilog(ConfigureAuditSerilog);

        string connectionString = _configuration.GetConnectionString(CurrencyApiConstants.DbConnectionString)!;
        services.AddDbContext<CurrencyPublicContext>(builder =>
                                                     {
                                                         builder.UseNpgsql(connectionString,
                                                                           static optionsBuilder =>
                                                                           {
                                                                               optionsBuilder.EnableRetryOnFailure();
                                                                               optionsBuilder.MigrationsHistoryTable(
                                                                                    HistoryRepository.DefaultTableName,
                                                                                    CurrencyApiConstants.SchemaName);
                                                                           });
                                                         builder.LogTo(Log.Debug,
                                                                       new[] { DbLoggerCategory.Database.Command.Name },
                                                                       LogLevel.Information);
                                                         builder.EnableSensitiveDataLogging();
                                                     });

        services.AddHealthChecks()
                .AddNpgSql(connectionString);

        services.AddControllers(static options => options.Filters.Add<ExceptionFilter>())

                 // Добавляем глобальные настройки для преобразования Json
                .AddJsonOptions(static options =>
                                {
                                    // Добавляем конвертер для енама
                                    // По умолчанию енам преобразуется в цифровое значение
                                    // Этим конвертером задаем перевод в строковое занчение
                                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                                });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(static options =>
                               {
                                   options.SwaggerDoc("v1",
                                                      new OpenApiInfo
                                                      {
                                                          Title       = "API",
                                                          Version     = "v1",
                                                          Description = "Test API",
                                                      });
                                   options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                                                                           $"{typeof(Program).Assembly.GetName().Name}.xml"),
                                                              true);
                               });
        services.AddHttpLogging(static options =>
                                {
                                    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.Response;
                                    options.MediaTypeOptions.AddText("application/json");
                                });

        var grpcAddress = _configuration.GetValue<string>(CurrencyApiConstants.GrpcAddressSettingsKey)!;
        services.AddGrpcClient<CurrencyApiGrpc.CurrencyApiGrpcClient>(options => options.Address = new Uri(grpcAddress))
                .AddAuditHandler(static configurator => configurator.IncludeRequestBody());

        services.AddTransient<ICurrencyApiService, CurrencyApiService>();

        return;

        static void ConfigureAuditSerilog(ISerilogConfigurator configurator)
        {
            configurator.Message(static @event =>
                                 {
                                     if (@event is not AuditEventHttpClient httpClientEvent)
                                     {
                                         return @event.ToJson();
                                     }

                                     object? contentBody = httpClientEvent.Action?.Response?.Content?.Body;
                                     if (contentBody is string { Length: > 1000 } body)
                                     {
                                         httpClientEvent.Action!.Response!.Content!.Body = body[..1000] + "<...>";
                                     }

                                     return @event.ToJson();
                                 });
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting()
           .UseEndpoints(static endpoints =>
                         {
                             endpoints.MapControllers();
                             endpoints.MapHealthChecks("/_health",
                                                       new HealthCheckOptions
                                                       {
                                                           ResponseWriter = WriteCheckResponse
                                                       });
                         });

        app.UseHttpLogging();
    }

    private static Task WriteCheckResponse(HttpContext httpContext, HealthReport healthReport)
    {
        switch (healthReport.Status)
        {
            case HealthStatus.Unhealthy:
                Log.Error("HealthCheck executed, result: UNHEALTHY");

                break;
            case HealthStatus.Degraded:
                Log.Warning("HealthCheck executed, result: DEGRADED");

                break;
            case HealthStatus.Healthy:
                Log.Information("HealthCheck executed, result: healthy");

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return UIResponseWriter.WriteHealthCheckUIResponse(httpContext, healthReport);
    }
}
