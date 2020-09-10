using System;
using System.Threading.Tasks;

namespace M.EventBroker.Async.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on a ThreadPool threads.
    /// </summary>
    public class UnrestrictedThreadPoolRunnerAsync : IEventHandlerRunnerAsync
    {
        /// <summary>
        /// Runs event handlers on a ThreadPool threads.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        public async Task RunAsync(params Func<Task>[] handlers)
        {
            foreach (Func<Task> handler in handlers)
            {
                var handler1 = handler;
                _ = Task.Run(async () => await handler1().ConfigureAwait(false));
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }
    }
}
