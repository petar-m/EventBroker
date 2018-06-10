using System;
using System.Threading.Tasks;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on a ThreadPool threads.
    /// </summary>
    public class UnrestrictedThreadPoolRunner : IEventHandlerRunner
    {
        /// <summary>
        /// Runs event handlers on a ThreadPool threads.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        public void Run(params Action[] handlers)
        {
            foreach (Action handler in handlers)
            {
                var handler1 = handler;
                Task.Run(handler1);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }
    }
}
