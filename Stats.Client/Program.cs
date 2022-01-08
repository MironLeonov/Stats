using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Stats.Protobuf.Sequence;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Threading;
using CsvHelper;

namespace Stats.Client
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(
                "http://localhost:18081",
                new GrpcChannelOptions()
                {
                    Credentials = ChannelCredentials.Insecure,
                    LoggerFactory = new NullLoggerFactory(),
                    MaxReceiveMessageSize = 100 * 1024 * 1024, // 100 MB
                    MaxSendMessageSize = 100 * 1024 * 1024 // 100 MB
                }
            );
            var cv = new CalculateValuesService.CalculateValuesServiceClient(channel);
            var met = new GetMetricsService.GetMetricsServiceClient(channel); 
            // await VerySimple(cv, 8);
            // await ArrayVersionSimple(cv, 8);
            // await GetDataFromPath(cv, 8,@"D:\values_100000_new.csv" );
            // await FromGenerateData(cv, 2);
            // await TestParallel(cv, 2);
            await TestMetrics(cv, met, 8); 
        }


        // private static void Main(string[] args)
        // {
        //     var values = ReadCsv(@"D:\values_100000.csv");
        //     foreach (var t in values)
        //     {
        //         Console.WriteLine(t);
        //     }
        // }

        private static List<double> GetValues(string path)
        {
            var values = new List<double>();
            try
            {
                using var reader = new StreamReader(path);
                while (!reader.EndOfStream)
                {
                    var strVal = reader.ReadLine();

                    values.Add(Convert.ToDouble(strVal));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return values;
        }
        
        
        public static List<double> ReadCsv(string absolutePath) {
            // List<string> result = new List<string>();
            string value;
            using var streamReader = new StreamReader(absolutePath);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            // csv.Configuration.HasHeaderRecord = false;
            // while (csv.Read()) {
            //     for(int i=0; csv.TryGetField<string>(i, out value); i++) {
            //         result.Add(value);
            //     }
            // }
            var result = csvReader.GetRecords<double>().ToList(); 

            return result;
        }


        private static async Task VerySimple(CalculateValuesService.CalculateValuesServiceClient cv, int cntThreads)
        {
            var result = await cv.CalculateValuesAsync(
                new Sequence()
                {
                    Values = {1, 2, 3},
                    CntThreads = cntThreads,
                    CorrelationId = "ad593c71-2c99-4f96-b931-fbc424f4bcc4"
                }
            );
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
        }

        private static async Task ArrayVersionSimple(CalculateValuesService.CalculateValuesServiceClient cv,
            int cntThreads)
        {
            var sequence = new Sequence()
            {
                Values = {0},
                CntThreads = cntThreads
            };
            var test = new List<double>() {1.0, 2, 3};
            sequence.Values.AddRange(test);
            var result = await cv.CalculateValuesAsync(
                sequence
            );
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
        }


        private static async Task GetDataFromPath(CalculateValuesService.CalculateValuesServiceClient cv,
            int cntThreads, string path)
        {
            var sequence = new Sequence()
            {
                CntThreads = cntThreads
            };
            
            var values = ReadCsv(path);
            
            Console.WriteLine(values.Count);
            
            foreach (var value in values)
            {
                sequence.Values.Add(value);
            }
            
            sequence.CorrelationId = Guid.NewGuid().ToString(); 
            
            var result = await cv.CalculateValuesAsync(
                sequence
            );

            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
            Console.WriteLine(sequence.Values.Count);
        }

        private static async Task FromGenerateData(CalculateValuesService.CalculateValuesServiceClient cv,
            int cntThreads)
        {
            var values = Enumerable.Range(0, 5_000).Select(_ => new Random().Next(-100, 100)).ToArray();

            var sequence = new Sequence()
            {
                CntThreads = cntThreads
            };

            foreach (var value in values)
            {
                sequence.Values.Add(value);
            }

            sequence.CorrelationId = Guid.NewGuid().ToString(); 

            var result = await cv.CalculateValuesAsync(
                sequence
            );

            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
            Console.WriteLine(sequence.Values.Count);
        }


        private static async Task TestParallel(CalculateValuesService.CalculateValuesServiceClient cv, int cntThreads)
        {
            var values = Enumerable.Range(0, 1_000).Select(_ => new Random().Next(-100, 100)).ToArray();

            var sequence = new Sequence()
            {
                CntThreads = cntThreads
            };

            foreach (var value in values)
            {
                sequence.Values.Add(value);
            }
            sequence.CorrelationId = Guid.NewGuid().ToString(); 
            
            var result = new Answer();
            
            var tasks = new List<Task>
            {
                Task.Run(
                    () =>
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            Console.WriteLine(i);
                        }
                    }
                ),
                Task.Run(
                    async () =>
                    {
                        result = await cv.CalculateValuesAsync(
                            sequence
                        );
                    }
                )
            };
            
            await Task.WhenAll(tasks);

            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
            Console.WriteLine(sequence.Values.Count);
        }

        private static async Task TestMetrics(CalculateValuesService.CalculateValuesServiceClient cv, GetMetricsService.GetMetricsServiceClient met, int cntThreads)
        {
            var values = Enumerable.Range(0, 1_000_000).Select(_ => new Random().Next(-100, 100)).ToList();

            var sequence = new Sequence()
            {
                CntThreads = cntThreads
            };

            foreach (var value in values)
            {
                sequence.Values.Add(value);
            }

            var result = new Answer();
            
            var corrId = Guid.NewGuid().ToString();
            sequence.CorrelationId = corrId;

            var cts = new CancellationTokenSource();
            
            var tasks = new List<Task>
            {
                Task.Run(
                    async () =>
                    {
                        try
                        {
                            using var streamingCall =
                                met.GetMetricsStream(new GuidForMetrics() {CorrelationId = corrId});
                            await foreach (var metricsData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                            {
                                for (var i = 0; i < metricsData.Values.Count; i++)
                                {
                                    Console.Write($"{i} -> {metricsData.Values[i]}; ");
                                    if (metricsData.Values.Sum() >= sequence.Values.Count)
                                    {
                                        cts.Cancel();
                                    }
                                }
                                Console.WriteLine();
                            }
                        }
                        catch(RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                        {               
                            Console.WriteLine("End Metrics.");
                        }
                    }
                ),
                Task.Run(
                    async () =>
                    {
                        result = await cv.CalculateValuesAsync(
                            sequence
                        );

                    }
                )
            };
            
            await Task.WhenAll(tasks);
            
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
            Console.WriteLine(sequence.Values.Count);
        }
    }
}