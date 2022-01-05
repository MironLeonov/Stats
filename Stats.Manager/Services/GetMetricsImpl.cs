using System;
using System.Threading.Tasks;
using Grpc.Core;
using Stats.Manager.Kafka;
using Stats.Protobuf.Sequence;
using Serilog;
using Stats.Protobuf.Metrics;


namespace Stats.Manager.Services
{
    public class GetMetricsImpl : GetMetricsService.GetMetricsServiceBase
    {
        public override async Task GetMetricsStream(GuidForMetrics request,
            IServerStreamWriter<MetricsProgress> responseStream, ServerCallContext context)
        {
            Log.Information("Handling new request metrics");
            Log.Information("CorrId Metrics: {CorrelationId}", request.CorrelationId);
            var guid = Guid.Parse(request.CorrelationId);
            KafkaAdapter.MetricsInit(guid);
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await foreach (var result in KafkaAdapter.MetricsConsume(guid))
                {
                    await responseStream.WriteAsync(result);
                }

                throw new Exception("Unreachable area");
            }
        }
    }
}