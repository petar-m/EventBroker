using System;
using System.Threading.Tasks;

namespace M.EventBroker
{
    // TODO: SubscribeAsync, UnsubscribeAsync - better naming!

    /// <summary>
    /// Represents an event broker.
    /// </summary>
    public interface IEventBroker : IDisposable
    {
        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate that will be invoked when event is published.</param>
        /// <param name="filter">A delegate used to perform filtering of events before invoking the handler.</param>
        /// <param name="onError">Called when an error is caught during execution.</param>
        void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter = null, Action<Exception, TEvent> onError = null);

        void SubscribeAsync<TEvent>(Func<TEvent, Task> handler, Func<TEvent, Task<bool>> filter = null, Func<Exception, TEvent, Task> onError = null);

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; which Handle method will be invoked when event is published.</param>
        void Subscribe<TEvent>(IEventHandler<TEvent> handler);

        void SubscribeAsync<TEvent>(IEventHandlerAsync<TEvent> handler);

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate to remove form subscribers.</param>
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        void UnsubscribeAsync<TEvent>(Func<TEvent, Task> handler);

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; to remove form subscribers.</param>
        void Unsubscribe<TEvent>(IEventHandler<TEvent> handler);

        void UnsubscribeAsync<TEvent>(IEventHandlerAsync<TEvent> handler);

        /// <summary>
        /// Publishes an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">An <typeparamref name="TEvent"/> instance to be passed to all handlers of the event.</param>
        void Publish<TEvent>(TEvent @event);

        Task PublishAsync<TEvent>(TEvent @event);
    }
}