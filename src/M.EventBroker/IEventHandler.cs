namespace M.EventBroker
{
    /// <summary>
    /// Represents a logic for handling events.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface IEventHandler<TEvent>
    {
        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="event"></param>
        void Handle(TEvent @event);

        /// <summary>
        /// Returns a value indicating whether the event handler should be executed.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        bool ShouldHandle(TEvent @event);
    }
}