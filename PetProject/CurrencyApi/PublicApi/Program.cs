using Microsoft.AspNetCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IWebHost webHost = WebHost
                          .CreateDefaultBuilder(args)
                          .UseStartup<Startup>()
                          .Build();

        await webHost.RunAsync();
    }

    // EF Core uses this method at design time to access the DbContext.
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(static webBuilder => webBuilder.UseStartup<Startup>());
}
