using NUnit.Framework;
using System;
using Grpc.Core;
using Grpc.Net.Client;
using Stats.Protobuf.Sequence;
using Microsoft.Extensions.Logging.Abstractions;

namespace Stats.Tests
{
    public class Tests
    {

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestBasic()
        {
            
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress(
                "http://localhost:18081",
                new GrpcChannelOptions()
                {
                    Credentials = ChannelCredentials.Insecure,
                    LoggerFactory = new NullLoggerFactory()
                }
            );
            var cv = new CalculateValuesService.CalculateValuesServiceClient(channel);
            var result =  cv.CalculateValues(
                new Sequence()
                {
                    Values = {5, 5, 5, 5, 5, 5},
                    CntThreads = 8
                }
            );
            Assert.AreEqual(5, result.EV); 
            Assert.AreEqual(0, result.Var);
        }
    }
}