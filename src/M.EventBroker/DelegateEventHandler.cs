using System;

namespace M.EventBroker
{
    public class DelegateEventHandler<TEvent> : IEventHandler<TEvent>
    {
        private readonly Action<TEvent> handler;
        private readonly Func<TEvent, bool> filter;

        public DelegateEventHandler(Action<TEvent> handler, Func<TEvent, bool> filter = null)
        {
            this.handler = handler;
            this.filter = filter;
        }

        public bool ShouldHandle(TEvent @event)
        {
            return filter == null || filter(@event);
        }

        public void Handle(TEvent @event)
        {
            handler(@event);
        }
    }
}

