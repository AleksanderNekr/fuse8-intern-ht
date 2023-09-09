using Fuse8_ByteMinds.SummerSchool.PublicApi.Data;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IHost webHost = Host
                       .CreateDefaultBuilder(args)
                       .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>())
                       .Build();

        await ApplyMigrationsAsync(webHost);

        await webHost.RunAsync();
    }

    private static async Task ApplyMigrationsAsync(IHost webHost)
    {
        using IServiceScope scope    = webHost.Services.CreateScope();
        IServiceProvider    services = scope.ServiceProvider;

        var context = services.GetRequiredService<CurrencyPublicContext>();
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }
    }
}
