using System;
using System.Threading.Tasks;

namespace M.EventBroker
{
    // TODO: fix comments

    /// <summary>
    /// Represents a logic for handling events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IEventHandlerAsync<TEvent>
    {
        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="event">An instance of TEvent representing the event.</param>
        Task HandleAsync(TEvent @event);

        /// <summary>
        /// Returns a value indicating whether the event handler should be executed.
        /// </summary>
        /// <param name="event">An instance of TEvent representing the event.</param>
        /// <returns>A value indicating whether the event handler should be executed.</returns>
        Task<bool> ShouldHandleAsync(TEvent @event);

        /// <summary>
        /// Called when an error is caught during execution.
        /// </summary>
        /// <param name="exception">The exception caught.</param>
        /// <param name="event">The event instance which handling caused the exception.</param>
        Task OnErrorAsync(Exception exception, TEvent @event);
    }
}