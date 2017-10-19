using System;

namespace M.EventBroker
{
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
        void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter = null);

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; which Handle method will be invoked when event is published.</param>
        void Subscribe<TEvent>(IEventHandler<TEvent> handler);

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate to remove form subscribers.</param>
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; to remove form subscribers.</param>
        void Unsubscribe<TEvent>(IEventHandler<TEvent> handler);

        /// <summary>
        /// Publishes an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">An <typeparamref name="TEvent"/> instance to be passed to all handlers of the event.</param>
        void Publish<TEvent>(TEvent @event);
    }
}