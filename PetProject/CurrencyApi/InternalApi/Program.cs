using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IWebHost webHost = WebHost
                          .CreateDefaultBuilder(args)
                          .UseStartup<Startup>()
                          .UseKestrel(static (context, options) =>
                                      {
                                          var grpcPort = context.Configuration.GetValue<int>(
                                           CurrencyApiConstants.GrpcPortSettingsKey);

                                          options.ConfigureEndpointDefaults(listenOptions =>
                                                                            {
                                                                                ConfigureHttpProtocol(
                                                                                 listenOptions,
                                                                                 grpcPort);
                                                                            });
                                      })
                          .Build();

        await ApplyMigrationsAsync(webHost);

        await webHost.RunAsync();

        return;

        static void ConfigureHttpProtocol(ListenOptions listenOptions, int grpcPort)
        {
            listenOptions.Protocols = listenOptions.IPEndPoint!.Port == grpcPort
                                          ? HttpProtocols.Http2
                                          : HttpProtocols.Http1;
        }
    }

    private static async Task ApplyMigrationsAsync(IWebHost webHost)
    {
        using IServiceScope scope    = webHost.Services.CreateScope();
        IServiceProvider    services = scope.ServiceProvider;

        var context = services.GetRequiredService<CurrencyInternalContext>();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
    }

    // EF Core uses this method at design time to access the DbContext.
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>());
}
