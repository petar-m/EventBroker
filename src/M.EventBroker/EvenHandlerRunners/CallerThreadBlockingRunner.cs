using System;
using System.Threading.Tasks;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on the thread as the caller, blocking it until all handlers are runned.
    /// </summary>
    public class CallerThreadBlockingRunner : IEventHandlerRunner
    {
        /// <summary>
        /// Runs event handlers on the thread as the caller, blocking it until all handlers are runned.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        public void Run(params Action[] handlers)
        {
            foreach (Action handler in handlers)
            {
                handler();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }
    }
}
