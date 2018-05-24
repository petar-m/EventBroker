using System.Collections.Generic;

namespace M.EventBroker
{
    /// <summary>
    /// Represents a provider for event handling instances.
    /// </summary>
    public interface IEventHandlerFactory
    {
        /// <summary>
        /// Returns an event handling instances.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <returns>An IEnumerable of event handlers. Null is valid return value.</returns>
        IEnumerable<IEventHandler<TEvent>> HandlersFor<TEvent>();
    }
}