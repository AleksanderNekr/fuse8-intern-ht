using Microsoft.AspNetCore;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi;

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
}
