//using Google.Protobuf.WellKnownTypes;
//using Grpc.Core;
//using Microsoft.AspNetCore.Authorization;
//using System;
//using System.Threading.Tasks;

//namespace Mip.Grpc.Example
//{
//    public class GreeterService : Greeter.GreeterBase
//    {
//        public GreeterService(IServiceProvider serviceProvider)
//        {
//            Console.WriteLine("Got SP: " + serviceProvider.GetHashCode());
//        }

//        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
//        {
//            return Task.FromResult(new HelloReply
//            {
//                Message = "OLD Hello " + request.Name
//            });
//        }

//        [Authorize()]
//        public override Task<HowdyReply> SayHowdy(HowdyRequest request, ServerCallContext context)
//        {
//            return Task.FromResult(new HowdyReply
//            {
//                Message = "OLD Howdy" + request.Name,
//                Number = request.Number + 1
//            });
//        }

//        public override Task<Empty> SayNothing(Empty request, ServerCallContext context)
//        {
//            return base.SayNothing(request, context);
//        }

//        public override Task<Empty> AskNothing(Empty request, ServerCallContext context)
//        {
//            return base.AskNothing(request, context);
//        }

//        public override Task SentenceToWords(SentenceMessage request, IServerStreamWriter<WordMessage> responseStream, ServerCallContext context)
//        {
//            return base.SentenceToWords(request, responseStream, context);
//        }

//        public override Task<SentenceMessage> WordsToSentence(IAsyncStreamReader<WordMessage> requestStream, ServerCallContext context)
//        {
//            return base.WordsToSentence(requestStream, context);
//        }

//        public override Task ReverseWords(IAsyncStreamReader<WordMessage> requestStream, IServerStreamWriter<ReverseWordMessage> responseStream, ServerCallContext context)
//        {
//            return base.ReverseWords(requestStream, responseStream, context);
//        }
//    }
//}
