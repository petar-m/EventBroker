using System;

namespace M.EventBroker
{
    /// <summary>
    /// An adapter used to represent delegate as IEventHandler.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to be handled.</typeparam>
    public class DelegateEventHandler<TEvent> : IEventHandler<TEvent>
    {
        private readonly Action<TEvent> _handler;
        private readonly Func<TEvent, bool> _filter;

        /// <summary>
        /// Creates a new instance of the DelegateEventHandler class.
        /// </summary>
        /// <param name="handler">A delegate used for event handling.</param>
        /// <param name="filter">A delegate used to determine whether the event should be handled.</param>
        public DelegateEventHandler(Action<TEvent> handler, Func<TEvent, bool> filter = null)
        {
            _handler = handler;
            _filter = filter;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="event">An instance of TEvent representing the event.</param>
        public void Handle(TEvent @event)
        {
            _handler(@event);
        }

        /// <summary>
        /// Returns a value indicating whether the event handler should be executed.
        /// </summary>
        /// <param name="event">An instance of TEvent representing the event.</param>
        /// <returns>A value indicating whether the event handler should be executed.</returns>
        public bool ShouldHandle(TEvent @event)
        {
            return _filter == null || _filter(@event);
        }
    }
}

