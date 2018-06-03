using System;

namespace M.EventBroker
{
    internal class EventHandlerWrapper<TEvent> : IEventHandler<TEvent>
    {
        private readonly IEventHandler<TEvent> _eventHandler;
        private readonly Action<TEvent> _handler;
        
        public EventHandlerWrapper(IEventHandler<TEvent> eventHandler)
        {
            _eventHandler = eventHandler;
            _handler = eventHandler.Handle;
            IsSubscribed = true;
        }

        public EventHandlerWrapper(Action<TEvent> handler, Func<TEvent, bool> filter = null, Action<Exception, TEvent> onError = null)
        {
            _eventHandler = new DelegateEventHandler<TEvent>(handler, filter, onError);
            _handler = handler;
            IsSubscribed = true;
        }

        public bool IsSubscribed
        {
            get; set;
        }

        public void Handle(TEvent @event)
        {
            _eventHandler.Handle(@event);
        }

        public bool ShouldHandle(TEvent @event)
        {
            return _eventHandler.ShouldHandle(@event);
        }

        public void OnError(Exception exception, TEvent @event)
        {
            _eventHandler.OnError(exception, @event);
        }

        public bool IsWrapping(IEventHandler<TEvent> eventHandler)
        {
            return _eventHandler == eventHandler;
        }

        public bool IsWrapping(Action<TEvent> handler)
        {
            return _handler == handler;
        }
    }
}