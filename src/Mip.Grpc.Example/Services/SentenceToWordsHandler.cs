using Grpc.Core;
using MiP.Grpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mip.Grpc.Example
{
    public class SentenceToWordsHandler : IServerStreamHandler<SentenceMessage, WordMessage>
    {
        public async Task RunAsync(IServerStreamRequest<SentenceMessage, WordMessage> request, ServerCallContext context)
        {
            var words = new List<string>(request.Request.Sentence.Split(" "));

            foreach (var word in words)
            {
                await request.Stream.WriteAsync(new WordMessage { Word = word }).ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
}
