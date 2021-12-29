using System;
using System.Threading.Tasks;
using Grpc.Core;
using Stats.Manager.Kafka;
using Stats.Protobuf.Sequence;
using Stats.Protobuf.Worker;
using Serilog;

namespace Stats.Manager.Services
{
     public class CalculateValuesServiceImpl : CalculateValuesService.CalculateValuesServiceBase
     {
          public override async Task<Answer> CalculateValues(Sequence request, ServerCallContext context)
          {
               Log.Information("Handling new request");
               var correlationId = Guid.NewGuid();
               Log.Information("Generated corrId: {CorrelationId}", correlationId);
               await KafkaAdapter.ProduceAsync(
                    correlationId,
                    new WorkerRq
                    {
                         CorrelationId = correlationId.ToString(),
                         RqData = request
                    }
               );
               Log.Information("Awaiting for result");
               await foreach (var result in KafkaAdapter.Consume(correlationId))
               {
                    Log.Information("Receiving result");
                    return result.RsData;
               }
               throw new Exception("Unreachable area");
          }
     }
}