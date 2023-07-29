using System.Text.Json.Serialization;
using Audit.Core;
using Audit.Http;
using Fuse8_ByteMinds.SummerSchool.PublicApi.AuditDataProviders;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Constants;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Settings;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

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

        services.AddTransient<ApiKeyHandler>();

        services.AddHttpClient(CurrencyApiConstants.DefaultClientName,
                               static client => client.BaseAddress = new Uri(CurrencyApiConstants.ApiBaseUri))
                .AddAuditHandler(static configurator =>
                                 {
                                     configurator.IncludeRequestBody()
                                                 .IncludeRequestHeaders()
                                                 .IncludeResponseBody()
                                                 .IncludeResponseHeaders()
                                                 .IncludeContentHeaders();
                                 })
                .AddHttpMessageHandler<ApiKeyHandler>();

        IConfigurationSection currenciesSection =
            _configuration.GetRequiredSection(CurrencyApiConstants.CurrenciesSectionName);
        services.Configure<CurrenciesSettings>(currenciesSection);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<CheckStatusMiddleware>();

        app.UseRouting()
           .UseEndpoints(static endpoints => endpoints.MapControllers());

        app.UseHttpLogging();

        Configuration.Setup()
                     .UseCustomProvider(new LoggerDataProvider(app.ApplicationServices
                                                                  .GetService<ILogger<LoggerDataProvider>>()!));
    }
}
