using System;
using System.Threading.Tasks;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on the thread as the caller, blocking it until all handlers are runned.
    /// </summary>
    public class ContinueAfterHandlersRunnerAsync : IEventHandlerRunnerAsync
    {
        /// <summary>
        /// Runs event handlers on the thread as the caller, blocking it until all handlers are runned.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        public async Task RunAsync(params Func<Task>[] handlers)
        {
            foreach (Func<Task> handler in handlers)
            {
                await handler().ConfigureAwait(false);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }
    }
}
