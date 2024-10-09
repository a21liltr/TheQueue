using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using TheQueue.Server.Core;
using TheQueue.Server.Core.Options;
using TheQueue.Server.Core.Services;

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
                    services.Configure<ConnectionOptions>(context.Configuration);

                    if (args.Length >= 1 && int.TryParse(args[0], out int port))
                        context.Configuration.Get<ConnectionOptions>().Port = port;

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