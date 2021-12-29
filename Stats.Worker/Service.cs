using System;
using Confluent.Kafka;
using Stats.Protobuf.Worker;
using Serilog;


namespace Stats.Worker
{
    public class Service
    {
        public static void Serve(
            string rqTopic,
            IConsumer<Null, WorkerRq> consumer,
            string rsTopic,
            IProducer<Null, WorkerRs> producer
        )
        {
            Log.Information("Subscribing on topic {RqTopic}", rqTopic);
            consumer.Subscribe(rqTopic);
            while (true)
            {
                var record = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (record is null) continue;
                Log.Information("Receive new message from request topic with corrid={CorrelationId}",
                    record.Message.Value.CorrelationId);
                var rsData = Calculate.CalculateValues(record.Message.Value.RqData); // отсюда нужно получать инфу о прогрессе и продюсить ее в кафку. Как это делать?)

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
    }
}