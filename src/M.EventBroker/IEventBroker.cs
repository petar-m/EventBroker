using System;

namespace M.EventBroker
{
    public interface IEventBroker : IDisposable
    {
        void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter = null);

        void Subscribe<TEvent>(IEventHandler<TEvent> handler);

        void Unsubscribe<TEvent>(Action<TEvent> handler);

        void Unsubscribe<TEvent>(IEventHandler<TEvent> handler);

        void Publish<TEvent>(TEvent @event);
    }
}