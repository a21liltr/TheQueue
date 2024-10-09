using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using TheQueue.Server.Core;
using TheQueue.Server.Core.Options;

public class Program
{
    private static void Main(string[] args)
    {
        try
        {
            Log.Warning("Application Starting.");

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    configBuilder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();
                })
                .ConfigureServices((context, services) =>
                {
                    if (args.Length == 3 && int.TryParse(args[0], out int pubPort) && int.TryParse(args[1], out int repPort))
                    {
                        services.Configure<ConnectionOptions>(option =>
                        {
                            option.PubPort = pubPort;
                            option.RepPort = repPort;
                            option.Server = args[2];
                        });
                    }
                    else
                    {
                        services.Configure<ConnectionOptions>(context.Configuration);
                        Log.Warning("Wrong or no arguments given, using default values");
                    }

                    services.AddCustomServices();
                })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration))
                .Build();

            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}