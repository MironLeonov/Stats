using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using Stats.Protobuf.Metrics;
using Stats.ProtoHelpers;
using Stats.Protobuf.Worker;


namespace Stats.Manager.Kafka
{
    public static class KafkaAdapter
    {
        public static async Task ProduceAsync(Guid guid, WorkerRq workerRq)
        {
            Results.TryAdd(guid, Channel.CreateBounded<WorkerRs>(1));
            MetricsValues.TryAdd(guid, Channel.CreateUnbounded<MetricsProgress>());
            await WorkerProducer.ProduceAsync(
                TopicRq,
                new Message<Null, WorkerRq>()
                {
                    Value = workerRq
                }
            );
        }


        public static void MetricsInit(Guid guid)
        {
            MetricsValues.TryAdd(guid, Channel.CreateUnbounded<MetricsProgress>());
        }

        private static ConcurrentDictionary<Guid, Channel<WorkerRs>> Results { get; } = new();

        private static ConcurrentDictionary<Guid, Channel<MetricsProgress>> MetricsValues { get; } = new();
        

        public static IAsyncEnumerable<WorkerRs> Consume(Guid correlationId)
        {
            return Results[correlationId].Reader.ReadAllAsync();
        }

        public static IAsyncEnumerable<MetricsProgress> MetricsConsume(Guid correlationId)
        {
            return MetricsValues[correlationId].Reader.ReadAllAsync();
        }

        private static IProducer<Null, WorkerRq> WorkerProducer { get; set; }
        private static IConsumer<Null, WorkerRs> WorkerConsumer { get; set; }

        private static IConsumer<Null, MetricsProgress> MetricsConsumer { get; set; }

        private static Thread ConsumingThread { get; set; }

        private static Thread MetricsConsumingThread { get; set; }
        

        private static string TopicRq { get; set; }

        private static string TopicRs { get; set; }

        private static string TopicMet { get; set; }

        public static void Initialize(
            string bootstrapServers,
            string topicRq,
            string topicRs,
            string topicMet,
            string groupId
        )
        {
            WorkerConsumer = new ConsumerBuilder<Null, WorkerRs>(
                    new ConsumerConfig()
                    {
                        BootstrapServers = bootstrapServers,
                        MessageMaxBytes = 45000000,
                        FetchMaxBytes = 50000000,
                        ReceiveMessageMaxBytes = 100000000,
                        MaxPartitionFetchBytes = 1000000000,
                        QueuedMaxMessagesKbytes = 100000,
                        GroupId = groupId
                    }
                )
                .SetValueDeserializer(new ProtoDeserializer<WorkerRs>())
                .Build();


            MetricsConsumer = new ConsumerBuilder<Null, MetricsProgress>(
                    new ConsumerConfig()
                    {
                        BootstrapServers = bootstrapServers,
                        MessageMaxBytes = 45000000,
                        FetchMaxBytes = 50000000,
                        ReceiveMessageMaxBytes = 100000000,
                        MaxPartitionFetchBytes = 1000000000,
                        QueuedMaxMessagesKbytes = 100000,
                        GroupId = groupId
                    }
                )
                .SetValueDeserializer(new ProtoDeserializer<MetricsProgress>())
                .Build();


            WorkerConsumer.Subscribe(topicRs);
            ConsumingThread = new Thread(
                async () =>
                {
                    while (true)
                    {
                        var result = WorkerConsumer.Consume(TimeSpan.FromMilliseconds(1000));
                        if (result is null) continue;
                        var guid = Guid.Parse(result.Message.Value.CorrelationId);
                        ChannelWriter<WorkerRs> writer;
                        try
                        {
                            writer = Results[guid].Writer;
                        }
                        catch
                        {
                            throw new Exception("Access results writer troubles");
                        }
                        
                        await writer.WriteAsync(result.Message.Value);
                        writer.Complete();
                        Results.TryRemove(guid, out _);
                    }
                }
            ) {IsBackground = true};
            ConsumingThread.Start();


            MetricsConsumer.Subscribe(topicMet);
            MetricsConsumingThread = new Thread(
                async () =>
                {
                    while (true)
                    {
                        var result = MetricsConsumer.Consume(TimeSpan.FromMilliseconds(100));
                        if (result is null) continue;
                        var guid = Guid.Parse(result.Message.Value.CorrelationId);
                        ChannelWriter<MetricsProgress> writer;
                        try
                        {
                            writer = MetricsValues[guid].Writer;
                        }
                        catch
                        {
                            throw new Exception("Access metrics writer troubles");
                        }

                        await writer.WriteAsync(result.Message.Value);
                    }
                }
            ) {IsBackground = true};
            MetricsConsumingThread.Start();

            WorkerProducer = new ProducerBuilder<Null, WorkerRq>(
                    new ProducerConfig()
                    {
                        BootstrapServers = bootstrapServers,
                        MessageMaxBytes = 100000000,
                        ReceiveMessageMaxBytes = 100000000,
                        MessageCopyMaxBytes = 100000000,
                        QueueBufferingMaxKbytes = 100000,
                    }
                )
                .SetValueSerializer(new ProtoSerializer<WorkerRq>())
                .Build();
            
            TopicRq = topicRq;
            TopicRs = topicRs;
            TopicMet = topicMet;
        }
    }
}