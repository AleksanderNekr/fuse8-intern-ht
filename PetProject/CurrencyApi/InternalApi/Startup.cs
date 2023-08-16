using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Audit.Serilog.Configuration;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Handlers;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Middlewares;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Settings;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.ApiServices;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Db;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Cache.Local;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Grpc;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;
using Serilog;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Fuse8_ByteMinds.SummerSchool.InternalApi;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        string connectionString = _configuration.GetConnectionString(CurrencyApiConstants.DbConnectionString)!;
        services.AddDbContext<CurrencyInternalContext>(builder =>
                                                       {
                                                           builder.UseNpgsql(connectionString,
                                                                             static optionsBuilder =>
                                                                             {
                                                                                 optionsBuilder.EnableRetryOnFailure();
                                                                                 optionsBuilder.MigrationsHistoryTable(
                                                                                  HistoryRepository.DefaultTableName,
                                                                                  CurrencyApiConstants.SchemaName);
                                                                             })
                                                                  .UseAllCheckConstraints();
                                                       });

        services.AddControllers()
                .AddJsonOptions(static options =>
                                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(static options =>
                               {
                                   options.SwaggerDoc("v1",
                                                      new OpenApiInfo
                                                      {
                                                          Title       = "Internal API",
                                                          Version     = "v1",
                                                          Description = "Test internal API",
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
        services.AddTransient<ApiKeyHandler>();

        var baseAddress = _configuration.GetValue<string>(CurrencyApiConstants.BaseApiAddressSettingsKey)!;
        services
           .AddHttpClient<ICurrencyApiService, CurrencyApiService>(client => client.BaseAddress = new Uri(baseAddress))
           .AddAuditHandler(static configurator =>
                            {
                                configurator.IncludeRequestBody()
                                            .IncludeRequestHeaders()
                                            .IncludeContentHeaders()
                                            .IncludeResponseHeaders()
                                            .IncludeResponseBody();
                            })
           .AddHttpMessageHandler<ApiKeyHandler>();

        services.AddTransient<ICachedCurrencyAPI, DbCachedCurrencyApi>();
        services.AddSingleton<LocalCacheWorkerService>();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration)
                                              .CreateLogger();
        services.AddLogging(static builder =>
                            {
                                builder.ClearProviders();
                                builder.AddSerilog(dispose: true);
                            });
        Configuration.Setup().UseSerilog(ConfigureAuditSerilog);

        IConfigurationSection currenciesSection = _configuration.GetSection(CurrencyApiConstants.CurrenciesSettingsKey);
        services.Configure<CurrenciesSettings>(currenciesSection);

        IConfigurationSection cacheSection = _configuration.GetSection(CurrencyApiConstants.CacheSettingsKey);
        services.Configure<CacheSettings>(cacheSection);

        services.AddGrpc();

        return;

        static void ConfigureAuditSerilog(ISerilogConfigurator configurator)
        {
            configurator.Message(static @event =>
                                 {
                                     if (@event is not AuditEventHttpClient eventHttpClient)
                                     {
                                         return @event.ToJson();
                                     }

                                     object? contentBody = eventHttpClient.Action?.Response?.Content?.Body;
                                     if (contentBody is string { Length: > 1000 } body)
                                     {
                                         eventHttpClient.Action!.Response!.Content!.Body = body[..1000] + "<...>";
                                     }

                                     return @event.ToJson();
                                 });
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting()
           .UseEndpoints(static endpoints => endpoints.MapControllers());

        app.UseHttpLogging();

        var grpcPort = _configuration.GetValue<int>(CurrencyApiConstants.GrpcPortSettingsKey);
        app.UseWhen(predicate: context => context.Connection.LocalPort == grpcPort,
                    configuration: static grpcBuilder =>
                                   {
                                       grpcBuilder.UseRouting();
                                       grpcBuilder.UseEndpoints(static builder =>
                                                                    builder.MapGrpcService<CurrencyGrpcService>());
                                   });
    }
}
