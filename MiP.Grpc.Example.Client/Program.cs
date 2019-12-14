using Grpc.Net.Client;
using Mip.Grpc.Example;
using System;
using System.Threading.Tasks;

namespace MiP.Grpc.Example.Client
{
    class Program
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
                    Name = "Michael"
                });

                Console.WriteLine(result1.Message);

                await Task.Delay(1000);

                var result2 = await client.SayHowdyAsync(new HowdyRequest
                {
                    Name = "Simon",
                    Number = 1
                });

                Console.WriteLine(result1.Message + Environment.NewLine + result2.Number);
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
