using System;
using BenchmarkDotNet.Running;

namespace Stats.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<AlgorithmBenchmarks>();
        }
    }
}