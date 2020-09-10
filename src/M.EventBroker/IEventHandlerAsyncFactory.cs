using System.Collections.Generic;

namespace M.EventBroker
{
    /// <summary>
    /// Represents a provider for event handling instances.
    /// </summary>
    public interface IEventHandlerAsyncFactory
    {
        /// <summary>
        /// Returns an event handling instances.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <returns>An IEnumerable of event handlers. Null is valid return value.</returns>
        IEnumerable<IEventHandlerAsync<TEvent>> AsyncHandlersFor<TEvent>();
    }
}
