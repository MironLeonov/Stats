using Stats.Manager.Kafka;
using Stats.Manager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Stats.Manager
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) { services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 100 * 1024 * 1024; // 100 MB
                options.MaxSendMessageSize = 100 * 1024 * 1024; // 100 MB 
            }
        ); }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapGrpcService<CalculateValuesServiceImpl>();
                    endpoints.MapGrpcService<GetMetricsImpl>();
                    endpoints.MapGet(
                        "/",
                        async context =>
                        {
                            await context.Response.WriteAsync(
                                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909"
                            );
                        }
                    );
                }
            );

            KafkaAdapter.Initialize(
                configuration["STATS_KAFKA_BOOTSTRAP_SERVERS"],
                configuration["STATS_TOPIC_RQ"] ?? "stats.worker.rq",
                configuration["STATS_TOPIC_RS"] ?? "stats.worker.rs",
                configuration["STATS_KAFKA_TOPIC_MET"] ?? "stats.worker.met",
                configuration["STATS_KAFKA_GROUP_ID"] ?? "stats.manager"
            );
        }
    }
}