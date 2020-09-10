using System;
using System.Threading.Tasks;

namespace M.EventBroker.Async
{
    internal class EventHandlerAsyncWrapper<TEvent> : IEventHandlerAsync<TEvent>
    {
        private readonly IEventHandlerAsync<TEvent> _eventHandler;
        private readonly Func<TEvent, Task> _handler;

        public EventHandlerAsyncWrapper(IEventHandlerAsync<TEvent> eventHandler)
        {
            _eventHandler = eventHandler;
            _handler = eventHandler.HandleAsync;
            IsSubscribed = true;
        }

        public EventHandlerAsyncWrapper(Func<TEvent, Task> handler, Func<TEvent, Task<bool>> filter = null, Func<Exception, TEvent, Task> onError = null)
        {
            _eventHandler = new DelegateEventHandlerAsync<TEvent>(handler, filter, onError);
            _handler = handler;
            IsSubscribed = true;
        }

        public bool IsSubscribed
        {
            get; set;
        }

        public Task HandleAsync(TEvent @event) => _eventHandler.HandleAsync(@event);

        public Task<bool> ShouldHandleAsync(TEvent @event) => _eventHandler.ShouldHandleAsync(@event);

        public Task OnErrorAsync(Exception exception, TEvent @event) => _eventHandler.OnErrorAsync(exception, @event);

        public bool IsWrapping(IEventHandlerAsync<TEvent> eventHandler) => _eventHandler == eventHandler;

        public bool IsWrapping(Func<TEvent, Task> handler) => _handler == handler;
    }
}
