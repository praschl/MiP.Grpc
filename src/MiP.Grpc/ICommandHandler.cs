using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Implemented to make a class handle a specific service method that does not return a result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request parameter of the method being handled.</typeparam>
    public interface ICommandHandler<TRequest>
    {
        /// <summary>
        /// Runs the command, passing the <paramref name="request"/> parameter.
        /// </summary>
        /// <param name="request">The request parameter.</param>
        /// <param name="context">The <see cref="ServerCallContext"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task RunAsync(TRequest request, ServerCallContext context);
    }
}
