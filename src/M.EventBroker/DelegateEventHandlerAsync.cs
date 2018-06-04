using System;
using System.Threading.Tasks;

namespace M.EventBroker
{
    // TODO: fix comments

    /// <summary>
    /// An adapter used to represent delegate as IEventHandler.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to be handled.</typeparam>
    public class DelegateEventHandlerAsync<TEvent> : IEventHandlerAsync<TEvent>
    {
        private readonly Func<TEvent, Task> _handler;
        private readonly Func<TEvent, Task<bool>> _filter;
        private readonly Func<Exception, TEvent, Task> _onError;

        /// <summary>
        /// Creates a new instance of the DelegateEventHandler class.
        /// </summary>
        /// <param name="handler">A delegate used for event handling.</param>
        /// <param name="filter">A delegate used to determine whether the event should be handled.</param>
        /// <param name="onError">A delegate called when an error is caught during execution.</param>
        public DelegateEventHandlerAsync(Func<TEvent, Task> handler, Func<TEvent, Task<bool>> filter = null, Func<Exception, TEvent, Task> onError = null)
        {
            _handler = handler;
            _filter = filter;
            _onError = onError;
        }

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="event">An instance of TEvent representing the event.</param>
        public async Task HandleAsync(TEvent @event)
        {
            await _handler(@event);
        }

        /// <summary>
        /// Called when an error is caught during execution.
        /// </summary>
        /// <param name="exception">The exception caught.</param>
        /// <param name="event">The event instance which handling caused the exception.</param>
        public async Task OnErrorAsync(Exception exception, TEvent @event)
        {
            if(_onError == null)
            {
                return;
            }

            await _onError(exception, @event);
        }

        /// <summary>
        /// Returns a value indicating whether the event handler should be executed.
        /// </summary>
        /// <param name="event">An instance of TEvent representing the event.</param>
        /// <returns>A value indicating whether the event handler should be executed.</returns>
        public async Task<bool> ShouldHandleAsync(TEvent @event)
        {
            if (_filter == null)
            {
                return true;
            }

            return await _filter(@event);
        }
    }
}

