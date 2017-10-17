using System;

namespace M.EventBroker
{
    internal class EventHandlerWrapper<TEvent> : IEventHandler<TEvent>
    {
        private readonly IEventHandler<TEvent> eventHandler;
        private readonly Action<TEvent> handler;

        public EventHandlerWrapper(IEventHandler<TEvent> eventHandler)
        {
            this.eventHandler = eventHandler;
            handler = eventHandler.Handle;
            IsSubscribed = true;
        }

        public EventHandlerWrapper(Action<TEvent> handler, Func<TEvent, bool> filter = null)
        {
            eventHandler = new DelegateEventHandler<TEvent>(handler, filter);
            this.handler = handler; 
            IsSubscribed = true;
        }

        public bool IsSubscribed
        {
            get; set;
        }

        public void Handle(TEvent @event)
        {
            eventHandler.Handle(@event);
        }

        public bool ShouldHandle(TEvent @event)
        {
            return eventHandler.ShouldHandle(@event);
        }

        public bool IsWrapping(IEventHandler<TEvent> eventHandler)
        {
            return this.eventHandler == eventHandler;
        }

        public bool IsWrapping(Action<TEvent> handler)
        {
            return this.handler == handler;
        }
    }
}