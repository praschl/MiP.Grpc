using Grpc.Core;
using MiP.Grpc;
using System;
using System.Threading.Tasks;

namespace Mip.Grpc.Example
{
    public class ReverseWordsHandler : IBidiStreamHandler<WordMessage, ReverseWordMessage>
    {
        public async Task RunAsync(IBidiStreamRequest<WordMessage, ReverseWordMessage> streamRequest, ServerCallContext context)
        {
            await foreach (var word in streamRequest.RequestStream.ReadAllAsync())
            {
                var chars = word.Word.ToCharArray();
                Array.Reverse(chars);
                var reverse = new string(chars);

                await streamRequest.ResponseStream.WriteAsync(new ReverseWordMessage { Word = reverse }).ConfigureAwait(false);
            }
        }
    }
}
