using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Stats.Protobuf.Sequence;
using Stats.Worker;


namespace Stats.Benchmarks
{
    [SimpleJob(
        RunStrategy.Throughput
    )]
    public class AlgorithmBenchmarks
    {
        public Sequence Sequence;
        
        [Params(1, 2, 4, 8, 10, 12, 14)] 
        public int CntThreads;


        [GlobalSetup]
        public void GlobalSetup()
        {
            var values = Enumerable.Range(0, 1_000_000).Select(_ => new Random().Next(-100, 100)).ToArray();

            this.Sequence = new Sequence()
            {
                CntThreads = this.CntThreads
            };
            
            foreach (var value in values)
            {
                this.Sequence.Values.Add(value);
            }
        }

        
        [Benchmark]
        public void BenchCalculateStats() { Calculate.CalculateValues( this.Sequence, null); }


    }
}