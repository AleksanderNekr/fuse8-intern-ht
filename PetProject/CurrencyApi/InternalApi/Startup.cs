using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Audit.Serilog.Configuration;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Filters;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Handlers;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Services.Contracts;
using Microsoft.AspNetCore.HttpLogging;
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
        services.AddControllers(static options => options.Filters.Add<ExceptionFilter>())
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
        services
           .AddHttpClient<ICurrencyAPI, CurrencyApiService>(client => client.BaseAddress = new Uri(baseAddress))
           .AddAuditHandler(static configurator =>
                            {
                                configurator.IncludeRequestBody()
                                            .IncludeRequestHeaders()
                                            .IncludeContentHeaders()
                                            .IncludeResponseHeaders()
                                            .IncludeResponseBody();
                            })
           .AddHttpMessageHandler<ApiKeyHandler>();

        services.AddTransient<ICachedCurrencyAPI, CachedCurrencyApi>();

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
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting()
           .UseEndpoints(static endpoints => endpoints.MapControllers());

        app.UseHttpLogging();
    }
}
