using System.Configuration;
using System.Text.Json.Serialization;
using Audit.Http;
using Fuse8_ByteMinds.SummerSchool.PublicApi.AuditDataProviders;
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
                                                          Description = "Test API"
                                                      });
                                   options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                                                                           $"{typeof(Program).Assembly.GetName().Name}.xml"),
                                                              true);
                               });
        services.AddHttpLogging(static options =>
                                {
                                    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.Response;
                                    options.MediaTypeOptions.AddText("application/javascript");
                                });

        Audit.Core.Configuration.Setup()
             .UseCustomProvider(new ConsoleDataProvider());

        services.AddHttpClient("DefaultClient",
                               client =>
                               {
                                   string apiKey = this._configuration.GetValue<string>("API_KEY")
                                                ?? throw new SettingsPropertyNotFoundException("API key not found");
                                   client.BaseAddress = new Uri("https://api.currencyapi.com/v3/");
                                   client.DefaultRequestHeaders.Add("apikey", apiKey);
                               })
                .AddAuditHandler(static configurator =>
                                 {
                                     configurator.IncludeRequestBody()
                                                 .IncludeRequestHeaders()
                                                 .IncludeResponseBody()
                                                 .IncludeResponseHeaders()
                                                 .IncludeContentHeaders();
                                 });

        IConfigurationSection currenciesSection = this._configuration.GetRequiredSection("Currencies");
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
    }
}
