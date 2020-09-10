using System;
using System.Threading.Tasks;

namespace M.EventBroker
{
    /// <summary>
    /// Represents an event broker.
    /// </summary>
    public interface IEventBrokerAsync : IDisposable
    {
        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate that will be invoked when event is published.</param>
        /// <param name="filter">A delegate used to perform filtering of events before invoking the handler.</param>
        /// <param name="onError">Called when an error is caught during execution.</param>
        void Subscribe<TEvent>(Func<TEvent, Task> handler, Func<TEvent, Task<bool>> filter = null, Func<Exception, TEvent, Task> onError = null);

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; which Handle method will be invoked when event is published.</param>
        void Subscribe<TEvent>(IEventHandlerAsync<TEvent> handler);

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate to remove form subscribers.</param>
        void Unsubscribe<TEvent>(Func<TEvent, Task> handler);

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; to remove form subscribers.</param>
        void Unsubscribe<TEvent>(IEventHandlerAsync<TEvent> handler);

        /// <summary>
        /// Publishes an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">An <typeparamref name="TEvent"/> instance to be passed to all handlers of the event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task PublishAsync<TEvent>(TEvent @event);
    }
}
