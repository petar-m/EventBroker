namespace M.EventBroker
{
    internal class InternalEventHandler<TEvent> : IEventHandler<TEvent>, IHandlerStatus
    {
        public InternalEventHandler(IEventHandler<TEvent> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public IEventHandler<TEvent> EventHandler { get; }

        public bool Unsubscribed
        {
            get; set;
        }

        public void Handle(TEvent @event)
        {
            EventHandler.Handle(@event);
        }

        public bool ShouldHandle(TEvent @event)
        {
            return EventHandler.ShouldHandle(@event);
        }
    }
}