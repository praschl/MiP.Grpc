using Grpc.Core;
using MiP.Grpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mip.Grpc.Example
{
    public class WordsToSentenceHandler : IClientStreamHandler<WordMessage, SentenceMessage>
    {
        public async Task<SentenceMessage> RunAsync(IAsyncStreamReader<WordMessage> request, ServerCallContext context)
        {
            List<string> words = new List<string>();

            await foreach (var word in request.ReadAllAsync())
            {
                words.Add(word.Word);
                Console.WriteLine($"Received word: {word.Word}");
            }

            return new SentenceMessage { Sentence = string.Join(" ", words) + "." };
        }
    }
}
