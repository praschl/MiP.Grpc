using Grpc.Core;
using System.Threading.Tasks;

namespace MiP.Grpc
{
    /// <summary>
    /// Implemented to make a class handle a specific service method that does not return a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command parameter of the method being handled.</typeparam>
    public interface ICommandHandler<TCommand>
    {
        /// <summary>
        /// Runs the command, passing the <paramref name="command"/> parameter.
        /// </summary>
        /// <param name="command">The request parameter.</param>
        /// <param name="context">The <see cref="ServerCallContext"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task RunAsync(TCommand command, ServerCallContext context);
    }
}
