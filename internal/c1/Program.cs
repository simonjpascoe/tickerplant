using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace Tp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client =  new TickerPlant.TickerPlantClient(channel);
            using (var call = client.Subscribe(new SubscriptionRequest()))
            {
              var t = new CancellationToken();
              while (await call.ResponseStream.MoveNext(t))
              {
                var message = call.ResponseStream.Current;
                Console.WriteLine($"{message.Value}");
              }
            }
            Console.ReadKey();
        }
    }
}
