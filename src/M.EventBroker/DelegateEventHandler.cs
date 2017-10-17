using System;

namespace M.EventBroker
{

    internal class DelegateEventHandler<TEvent> : IEventHandler<TEvent>, IHandlerStatus
    {
        private readonly Func<TEvent, bool> filter;

        public DelegateEventHandler(Action<TEvent> handler, Func<TEvent, bool> filter = null)
        {
            Handler = handler;
            this.filter = filter;
        }

        public bool ExecuteHandler(TEvent @event)
        {
            return filter == null || filter(@event);
        }

        public void Handle(TEvent @event)
        {
            Handler(@event);
        }

        public Action<TEvent> Handler { get; }

        public bool Unsubscribed
        {
            get; set;
        }
    }
}

