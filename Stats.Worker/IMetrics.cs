using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Confluent.Kafka;
using Stats.Protobuf.Worker;
using Stats.Protobuf.Metrics;
using Serilog;


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