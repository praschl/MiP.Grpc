using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Mip.Grpc.Example;
using Mip.Grpc.Example.Calc;
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

                var calc = new Calculator.CalculatorClient(channel);

                var result1 = await client.SayHelloAsync(new HelloRequest
                {
                    Name = "Michael 1"
                });
                Console.WriteLine(result1.Message);
                await Task.Delay(100);

                result1 = await client.SayHelloAsync(new HelloRequest
                {
                    Name = "Michael 2"
                });
                Console.WriteLine(result1.Message);
                await Task.Delay(100);

                var result2 = await client.SayHowdyAsync(new HowdyRequest
                {
                    Name = "Simon 1",
                    Number = 1
                });
                Console.WriteLine(result2.Message + "-" + result2.Number);
                await Task.Delay(100);

                result2 = await client.SayHowdyAsync(new HowdyRequest
                {
                    Name = "Simon 2",
                    Number = 1
                });
                Console.WriteLine(result2.Message + "-" + result2.Number);
                await Task.Delay(100);

                await client.AskNothingAsync(new Empty());
                Console.WriteLine("nothing asked");
                await Task.Delay(100);

                await client.SayNothingAsync(new Empty());
                Console.WriteLine("nothing said");
                await Task.Delay(100);

                await client.SayRandomAsync(new Empty());
                Console.WriteLine("something random said");
                await Task.Delay(100);

                await client.SayOneAsync(new OneCommand { One = "Eins" });
                Console.WriteLine("One said");
                await Task.Delay(100);

                await client.SayTwoAsync(new Empty());
                Console.WriteLine("Two said");
                await Task.Delay(100);

                var calcResult = await calc.AddAsync(new AddRequest { A = 2, B = 3 });
                Console.WriteLine("Added: " + calcResult.Res);
                await Task.Delay(100);

                await client.DuplicateAsync(new Empty());
                Console.WriteLine("DuplicateAsync for greeter called");
                await Task.Delay(100);

                await calc.DuplicateAsync(new Empty());
                Console.WriteLine("DuplicateAsync for calc called");
                await Task.Delay(100);

                var stwCall = client.SentenceToWords(new SentenceMessage { Sentence = "This is a fine sentence." });
                await foreach (var word in stwCall.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine($"Received word: <{word.Word}>");
                }

                var clientStream = client.WordsToSentence();
                await clientStream.RequestStream.WriteAsync(new WordMessage { Word = "Sending" });
                await Task.Delay(100);
                await clientStream.RequestStream.WriteAsync(new WordMessage { Word = "words" });
                await Task.Delay(100);
                await clientStream.RequestStream.WriteAsync(new WordMessage { Word = "one" });
                await Task.Delay(100);
                await clientStream.RequestStream.WriteAsync(new WordMessage { Word = "by" });
                await Task.Delay(100);
                await clientStream.RequestStream.WriteAsync(new WordMessage { Word = "one" });
                await Task.Delay(100);
                await clientStream.RequestStream.CompleteAsync();
                await Task.Delay(100);
                var sentence = await clientStream.ResponseAsync;
                Console.WriteLine($"WordsToSentence resulted in '{sentence.Sentence}'");

                // bidi stream
                var reverseStream = client.ReverseWords();
                await reverseStream.RequestStream.WriteAsync(new WordMessage { Word = "Sending" });
                await reverseStream.ResponseStream.MoveNext();
                var w = reverseStream.ResponseStream.Current;
                Console.WriteLine($"Reversed: {w.Word}");
                await Task.Delay(100);

                await reverseStream.RequestStream.WriteAsync(new WordMessage { Word = "Words" });
                await reverseStream.ResponseStream.MoveNext();
                w = reverseStream.ResponseStream.Current;
                Console.WriteLine($"Reversed: {w.Word}");
                await Task.Delay(100);

                await reverseStream.RequestStream.WriteAsync(new WordMessage { Word = "for" });
                await reverseStream.ResponseStream.MoveNext();
                w = reverseStream.ResponseStream.Current;
                Console.WriteLine($"Reversed: {w.Word}");
                await Task.Delay(100);

                await reverseStream.RequestStream.WriteAsync(new WordMessage { Word = "reversing!" });
                await reverseStream.ResponseStream.MoveNext();
                w = reverseStream.ResponseStream.Current;
                Console.WriteLine($"Reversed: {w.Word}");
                await Task.Delay(100);

                await reverseStream.RequestStream.CompleteAsync();
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
