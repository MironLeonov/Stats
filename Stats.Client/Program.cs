using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Stats.Protobuf.Sequence;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using Google.Protobuf.Collections;

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
            // await VerySimple(cv, 8);
            // await ArrayVersionSimple(cv, 8);
            // await GetDataFromPath(cv, 8,@"D:\values_big.csv" );
            await FromGenerateData(cv, 2); 

        }

        // private static void Main(string[] args)
        // {
            // var values = GetValues(@"D:\values.csv");
            //
            // foreach (var value in values)
            // {
            //     Console.WriteLine(value);
            // }
            
            // var test = GetValues(@"D:\values_50.csv");
            // foreach (var i in test)
            // {
            //     Console.WriteLine(i);
            // }
            // test = GetValues(@"D:\values_50_new.csv");
            // foreach (var i in test)
            // {
            //     Console.WriteLine(i);
            // }
            //
            // var sequence = new Sequence()
            // {
            //     Values = { }, CntThreads = 3
            // }; 
            // sequence.Values.AddRange(test);
            // Console.WriteLine(sequence.Values);

            // string test = "5,4"; 
            // Console.WriteLine(Convert.ToDouble(test));
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


        private static async Task VerySimple(CalculateValuesService.CalculateValuesServiceClient cv, int cntThreads)
        {
            var result = await cv.CalculateValuesAsync(
                    new Sequence()
                    {
                        Values = {1,2,3},
                        CntThreads = cntThreads
                    }
                );
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
        }
        
        private static async Task ArrayVersionSimple(CalculateValuesService.CalculateValuesServiceClient cv, int cntThreads)
        {
            var sequence = new Sequence()
            {
                Values = {0},
                CntThreads = cntThreads
            };
            var test = new List<double>(){1.0, 2, 3};
            sequence.Values.AddRange(test);
            var result = await cv.CalculateValuesAsync(
                sequence
            );
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
        }
        
        
        
        private static async Task GetDataFromPath(CalculateValuesService.CalculateValuesServiceClient cv, int cntThreads, string path)
        {
            var sequence = new Sequence()
            {
                Values = {0},
                CntThreads = cntThreads
            };
            var values = GetValues(path);
            sequence.Values.AddRange(values);
            var result = await cv.CalculateValuesAsync(
                sequence
            );
            sequence.Values.Capacity = values.Count + 1;
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
            Console.WriteLine(sequence.Values.Count);
        }

        private static async Task FromGenerateData(CalculateValuesService.CalculateValuesServiceClient cv,
            int cntThreads)
        {
            var values = Enumerable.Range(0, 5000000).Select(_ => new Random().Next(-100, 100)).ToArray();

            var sequence = new Sequence()
            {
                CntThreads = cntThreads
            };

            foreach (var value in values)
            {
                sequence.Values.Add(value);
            }
            
            var result = await cv.CalculateValuesAsync(
                sequence
            );
            // sequence.Values.Capacity = values.Count + 1;
            Console.WriteLine($"EV {result.EV}");
            Console.WriteLine($"Var {result.Var}");
            Console.WriteLine($"Time {result.Time}");
            Console.WriteLine(sequence.Values.Count);
        }
        
    }
}