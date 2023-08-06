using Fuse8_ByteMinds.SummerSchool.InternalApi.Constants;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

        await webHost.RunAsync();

        return;

        static void ConfigureHttpProtocol(ListenOptions listenOptions, int grpcPort)
        {
            listenOptions.Protocols = listenOptions.IPEndPoint!.Port == grpcPort
                                          ? HttpProtocols.Http2
                                          : HttpProtocols.Http1;
        }
    }
}
