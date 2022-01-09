using System;
using Confluent.Kafka;
using Stats.ProtoHelpers;
using Stats.Protobuf.Worker;
using Serilog;
using Stats.Protobuf.Metrics;

namespace Stats.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting...");
            var bootstrapServers = Environment.GetEnvironmentVariable("STATS_KAFKA_BOOTSTRAP_SERVERS") 
                                   ?? "kafka:29092";
            var groupId = Environment.GetEnvironmentVariable("STATS_KAFKA_GROUP_ID") ?? "stats.workers2";
            var rqTopic = Environment.GetEnvironmentVariable("STATS_KAFKA_TOPIC_RQ") ?? "stats.worker.rq";
            var rsTopic = Environment.GetEnvironmentVariable("STATS_KAFKA_TOPIC_RS") ?? "stats.worker.rs";
            var metTopic = Environment.GetEnvironmentVariable("STATS_KAFKA_TOPIC_MET") ?? "stats.worker.met"; 
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Latest
            };
            using var consumer = new ConsumerBuilder<Null, WorkerRq>(consumerConfig)
                .SetValueDeserializer(new ProtoDeserializer<WorkerRq>())
                .Build();
            
            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = bootstrapServers
            };
            
            using var producer = new ProducerBuilder<Null, WorkerRs>(producerConfig)
                .SetValueSerializer(new ProtoSerializer<WorkerRs>())
                .Build();


            using var producerMet = new ProducerBuilder<Null, MetricsProgress>(producerConfig)
                .SetValueSerializer(new ProtoSerializer<MetricsProgress>())
                .Build();
            
            Log.Information("Serving...");
            Service.Serve(rqTopic, consumer, rsTopic, producer, metTopic, producerMet);
            Log.CloseAndFlush();
        }
    }
}