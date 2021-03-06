#nullable enable
using System;
using Stats.Protobuf.Sequence;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Stats.Worker
{
    public class Calculate
    {
        public static Answer CalculateValues(Sequence seq, IMetrics? metricsCalc)
        {
            var result = new Answer
            {
                EV = 0,
                Var = 0,
                Time = ""
            };

            var stopWatch = new Stopwatch();
            metricsCalc?.Init(); 
            stopWatch.Start();
            
            
            var expValue = ExpectedValue(seq, metricsCalc);

            result.EV = expValue;

            result.Var = Variance(seq, expValue).Result;

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);

            result.Time = elapsedTime;

            return result;
        }

        public static async Task<double> Variance(Sequence seq, double ev)
        {
            var length = seq.Values.Count;

            var cntGroups = seq.CntThreads;

            var wide = length / cntGroups + 1;


            var sums = new ConcurrentBag<double>();
            var tasks = Enumerable.Range(0, cntGroups).Select(
                i => Task.Run(
                    () =>
                    {
                        var from = i * wide;
                        var to = Math.Min(length, from + wide);
                        var acc = 0.0;
                        for (var j = from; j < to; j++)
                        {
                            acc += (seq.Values[j] - ev) * (seq.Values[j] - ev);
                        }

                        acc /= length;

                        sums.Add(acc);
                    }
                )
            ).ToList();

            await Task.WhenAll(tasks);

            var variance = sums.ToArray().Sum();

            return variance;
        }


        public static double ExpectedValue(Sequence seq, IMetrics? metricsCalc)
        {
            var length = seq.Values.Count;

            var cntGroups = seq.CntThreads;

            var wide = length / cntGroups + 1;

            var sums = new ConcurrentBag<double>();

            Parallel.ForEach(
                Enumerable.Range(0, cntGroups),
                i =>
                {
                    var from = i * wide;
                    var to = Math.Min(length, from + wide);
                    var acc = 0.0;
                    var step = from; 
                    for (var j = from; j < to; j++)
                    {
                        if (j - step == 100)
                        {
                            metricsCalc?.GetMetrics(i, 100);
                            step = j; 
                        }
                        acc += seq.Values[j];
                    }
                    
                    var diff = (to - from) % 100;
                    metricsCalc?.GetMetrics(i, diff);
                    
                    acc /= length;
                    sums.Add(acc);
                }
            );


            var expValue = sums.ToArray().Sum();

            return expValue;
        }


        //     public static double ExpectedValue(Sequence values)
        //     {
        //         var seq = values.Values;
        //         var length = seq.Count;
        //
        //         var cntGroups = Environment.ProcessorCount;
        //
        //         var countdown = new CountdownEvent(cntGroups);
        //
        //         var wide = length / cntGroups + 1;
        //
        //
        //         var sums = new ConcurrentBag<int>();
        //         var threads = Enumerable.Range(0, cntGroups).Select(
        //             i => new Thread(
        //                 () =>
        //                 {
        //                     var from = i * wide;
        //                     var to = Math.Min(length, from + wide);
        //                     var acc = 0;
        //                     for (var j = from; j < to; j++)
        //                     {
        //                         acc += seq[j];
        //                     }
        //
        //                     sums.Add(acc);
        //
        //                     countdown.Signal();
        //                 }
        //             )
        //         ).ToList();
        //         foreach (var thread in threads)
        //         {
        //             thread.Start();
        //         }
        //
        //         countdown.Wait();
        //
        //         var expValue = (double) sums.ToArray().Sum() / length;
        //         return expValue;
        //     }
        //
        //
        //     public static double Variance(Sequence values, double ev)
        //     {
        //         var seq = values.Values;
        //         var length = seq.Count;
        //
        //         var cntGroups = Environment.ProcessorCount;
        //
        //         var countdown = new CountdownEvent(cntGroups);
        //
        //         var wide = length / cntGroups + 1;
        //
        //
        //         var sums = new ConcurrentBag<double>();
        //         var threads = Enumerable.Range(0, cntGroups).Select(
        //             i => new Thread(
        //                 () =>
        //                 {
        //                     var from = i * wide;
        //                     var to = Math.Min(length, from + wide);
        //                     var acc = 0.0;
        //                     for (var j = from; j < to; j++)
        //                     {
        //                         acc += (seq[j] - ev) * (seq[j] - ev);
        //                     }
        //
        //                     sums.Add(acc);
        //
        //                     countdown.Signal();
        //                 }
        //             )
        //         ).ToList();
        //         foreach (var thread in threads)
        //         {
        //             thread.Start();
        //         }
        //
        //         countdown.Wait();
        //
        //         var variance = sums.ToArray().Sum() / length;
        //
        //         return variance;
        //     }
        // }
    }
}