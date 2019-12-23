using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Mip.Grpc.Example;
using System;
using System.Threading.Tasks;

namespace MiP.Grpc.Example.Client
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await Task.Delay(1000);

                var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Greeter.GreeterClient(channel);

                var result1 = await client.SayHelloAsync(new HelloRequest
                {
                    Name = "Michael 1"
                });
                Console.WriteLine(result1.Message);
                await Task.Delay(200);

                result1 = await client.SayHelloAsync(new HelloRequest
                {
                    Name = "Michael 2"
                });
                Console.WriteLine(result1.Message);
                await Task.Delay(200);

                var result2 = await client.SayHowdyAsync(new HowdyRequest
                {
                    Name = "Simon 1",
                    Number = 1
                });
                Console.WriteLine(result2.Message + "-" + result2.Number);
                await Task.Delay(200);

                result2 = await client.SayHowdyAsync(new HowdyRequest
                {
                    Name = "Simon 2",
                    Number = 1
                });
                Console.WriteLine(result2.Message + "-" + result2.Number);
                await Task.Delay(200);

                await client.AskNothingAsync(new Empty());
                Console.WriteLine("nothing asked");
                await Task.Delay(200);

                await client.SayNothingAsync(new Empty());
                Console.WriteLine("nothing said");
                await Task.Delay(200);

                await client.SayRandomAsync(new Empty());
                Console.WriteLine("something random said");
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("Press Enter");
                Console.ReadLine();
            }
        }
    }
}
