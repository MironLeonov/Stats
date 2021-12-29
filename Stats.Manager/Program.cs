using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;


namespace Stats.Manager
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            Log.Information("Starting...");
            var statsManagerPort = Environment.GetEnvironmentVariable("STATS_MANAGER_PORT") ?? "18081";
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.ConfigureKestrel(
                            options =>
                            {
                                options.Limits.MaxRequestBodySize = 50000000;
                                options.Limits.MaxRequestBufferSize = 50000000;
                                options.Limits.MaxResponseBufferSize = null; 
                                options.ListenAnyIP(
                                    Convert.ToInt32(statsManagerPort),
                                    o => o.Protocols = HttpProtocols.Http2
                                );
                            }
                        );
                        webBuilder.UseStartup<Startup>();
                    }
                );
        }
    }
}