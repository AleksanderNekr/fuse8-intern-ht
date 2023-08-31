using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IWebHost webHost = WebHost
                          .CreateDefaultBuilder(args)
                          .UseStartup<Startup>()
                          .Build();

        await ApplyMigrationsAsync(webHost);

        await webHost.RunAsync();
    }

    private static async Task ApplyMigrationsAsync(IWebHost webHost)
    {
        using IServiceScope scope    = webHost.Services.CreateScope();
        IServiceProvider    services = scope.ServiceProvider;

        var context = services.GetRequiredService<CurrencyPublicContext>();
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
