using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Stats.Avalonia.App.Models;
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
using System.Threading;

namespace Stats.Avalonia.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel() {ExpValue = "Waiting for result", Variance = "Waiting for result", Metrics = "Waiting for result", ElapsedTime = "Waiting for result"}; 
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


        public async void StartClicked(object sender, RoutedEventArgs eventArgs)
        {
            var context = this.DataContext as ViewModel;

            
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
    
            var sequence = new Sequence()
            {
                CntThreads = int.Parse(context.CountThreads)
            };

            if (context.Count != "")
            {
                var values = Enumerable.Range(0, int.Parse(context.Count)).Select(_ => new Random().Next(-100, 100)).ToArray();
                
                foreach (var value in values)
                {
                    sequence.Values.Add(value);
                }
            }
            else
            {
                var values = Enumerable.Range(0, 5).Select(_ => new Random().Next(-100, 100)).ToArray();
                
                foreach (var value in values)
                {
                    sequence.Values.Add(value);
                }
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
                        context.Metrics = "Starting Metrics"; 
                        context.Progress = 0;
                        try
                        {
                            using var streamingCall =
                                met.GetMetricsStream(new GuidForMetrics() {CorrelationId = corrId});
                            await using StreamWriter file = new(@"D:\ProjectsMEPhI\parProg\Stats\Stats.Avalonia.App\MetricsStats.txt");
                            await foreach (var metricsData in streamingCall.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                            {
                                var metrics = ""; 
                                for (var i = 0; i < metricsData.Values.Count; i++)
                                {
                                    metrics +=  $"{i} -> {metricsData.Values[i]}; ";

                                    if (metricsData.Values.Sum() >= sequence.Values.Count)
                                    {
                                        cts.Cancel();
                                    }

                                    context.Progress = metricsData.Values.Sum() / sequence.Values.Count * 100; 
                                }

                                if (context.IsChecked)
                                {
                                    await file.WriteLineAsync(metrics); 
                                }
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
            context.Metrics = "Hello"; 
            context.ExpValue = $"{Math.Round(result.EV, 3)}";
            context.Variance = $"{Math.Round(result.Var, 3)}";
            context.ElapsedTime = $"{result.Time}";
            if (context.IsChecked)
            {
                context.Metrics = "Check file for more information";
            }
        }
    }
}