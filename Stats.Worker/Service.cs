using System;
using System.Collections.Concurrent;
using System.Linq;
using Confluent.Kafka;
using Stats.Protobuf.Worker;
using Serilog;
using Stats.Protobuf.Metrics;


namespace Stats.Worker
{
    public class Service
    {
        public static void Serve(
            string rqTopic,
            IConsumer<Null, WorkerRq> consumer,
            string rsTopic,
            IProducer<Null, WorkerRs> producer,
            string metTopic,
            IProducer<Null, MetricsProgress> producerMet
        )
        {
            Log.Information("Subscribing on topic {RqTopic}", rqTopic);
            consumer.Subscribe(rqTopic);
            while (true)
            {
                var record = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (record is null) continue;
                Log.Information("Receive new message from request topic with corrId={CorrelationId}",
                    record.Message.Value.CorrelationId);

                
                var metricsProgressService = new MetricsProgressService()
                {
                    MetricsTopic = metTopic,
                    CorrelationId = record.Message.Value.CorrelationId,
                    MetricsProgressProducer = producerMet
                };


                var rsData =
                    Calculate.CalculateValues(record.Message.Value.RqData,
                        metricsProgressService); 

                var response = new WorkerRs
                {
                    RsData = rsData,
                    CorrelationId = record.Message.Value.CorrelationId
                };
                Log.Information("Sending result to response topic");
                producer.Produce(
                    rsTopic,
                    new Message<Null, WorkerRs>
                    {
                        Value = response
                    }
                );
            }
        }

        private class MetricsProgressService : IMetrics
        {
            public string MetricsTopic { get; init; }
            public string CorrelationId { get; init; }
            public IProducer<Null, MetricsProgress> MetricsProgressProducer { get; init; }

            public void Init()
            {
                _metrics = new ConcurrentDictionary<int, int>();
            }

            public void GetMetrics(int idx, int cnt)
            {
                _metrics.AddOrUpdate(idx, cnt, (k, v) => v + cnt);
                var value = new MetricsProgress()
                {
                    CorrelationId = this.CorrelationId
                };

                foreach (var i in _metrics.Values)
                {
                    value.Values.Add(i);
                }
                Log.Information("Sending Metrics with corrId={CorrelationId}", CorrelationId);
                MetricsProgressProducer.Produce(
                    MetricsTopic,
                    new Message<Null, MetricsProgress>()
                    {
                        Value = value
                    }
                );
                var answer = _metrics.Aggregate("", (current, pair) => current + pair.ToString());
                Log.Information(answer);
            }


            private ConcurrentDictionary<int, int> _metrics;
        }
    }
}