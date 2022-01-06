using Confluent.Kafka;
using Stats.Protobuf.Metrics;


namespace Stats.Worker
{
    public interface IMetrics
    {
        string MetricsTopic { get; }
        string CorrelationId { get; }
        IProducer<Null, MetricsProgress> MetricsProgressProducer { get; }
        void Init();
        void GetMetrics(int idx, int cnt);
    }
}