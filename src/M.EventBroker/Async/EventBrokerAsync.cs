using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace M.EventBroker.Async
{
    /// <summary>
    /// Manages event subscriptions and invoking of event handlers.
    /// </summary>
    public class EventBrokerAsync : IEventBrokerAsync
    {
        private readonly ConcurrentDictionary<Type, List<object>> _subscribers = new ConcurrentDictionary<Type, List<object>>();
        private readonly IEventHandlerAsyncFactory _handlersFactory;
        private readonly IEventHandlerRunnerAsync _runner;

        /// <summary>
        /// Creates a new instance of the EventBrokerAsync class.
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="handlersFactory">A delegate providing event handlers for event of givent type.</param>
        public EventBrokerAsync(IEventHandlerRunnerAsync runner, IEventHandlerAsyncFactory handlersFactory = null)
        {
            _runner = runner;
            _handlersFactory = handlersFactory;
        }

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate that will be invoked when event is published.</param>
        /// <param name="filter">A delegate used to perform filtering of events before invoking the handler.</param>
        /// <param name="onError">A delegate called when an error is caught during execution.</param>
        public void Subscribe<TEvent>(Func<TEvent, Task> handler, Func<TEvent, Task<bool>> filter = null, Func<Exception, TEvent, Task> onError = null)
        {
            var handlers = _subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerAsyncWrapper<TEvent>(handler, filter, onError));
        }

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; which Handle method will be invoked when event is published.</param>
        public void Subscribe<TEvent>(IEventHandlerAsync<TEvent> handler)
        {
            var handlers = _subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerAsyncWrapper<TEvent>(handler));
        }

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate to remove form subscribers.</param>
        public void Unsubscribe<TEvent>(Func<TEvent, Task> handler)
        {
            Unsubscribe<TEvent>(x => x.IsWrapping(handler));
        }

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; to remove form subscribers.</param>
        public void Unsubscribe<TEvent>(IEventHandlerAsync<TEvent> handler)
        {
            Unsubscribe<TEvent>(x => x.IsWrapping(handler));
        }

        /// <summary>
        /// Publishes an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">An <typeparamref name="TEvent"/> instance to be passed to all handlers of the event. All handlers will be invoked asynchronously.</param>
        public async Task PublishAsync<TEvent>(TEvent @event)
        {
            await EnqueueSubscribers(@event).ConfigureAwait(false);
            await EnqueueFromHandlersFactory(@event).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the EventBroker class.
        /// </summary>
        public void Dispose() => _runner.Dispose();

        private void Unsubscribe<TEvent>(Func<EventHandlerAsyncWrapper<TEvent>, bool> handlerPredicate)
        {
            var handlers = _subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            var targetHandlers = handlers.Cast<EventHandlerAsyncWrapper<TEvent>>().ToArray();
            foreach (var handlerAction in targetHandlers.Where(x => handlerPredicate(x)))
            {
                handlers.Remove(handlerAction);
                handlerAction.IsSubscribed = false;
            }
        }

        private async Task EnqueueSubscribers<TEvent>(TEvent @event)
        {
            bool hasSubscribers = _subscribers.TryGetValue(typeof(TEvent), out List<object> handlers);
            if (!hasSubscribers)
            {
                return;
            }

            if (handlers.Count == 0)
            {
                return;
            }

            Func<Task> CreateHandlerAction(EventHandlerAsyncWrapper<TEvent> handler)
            {
                return async () =>
                {
                    if (!handler.IsSubscribed)
                    {
                        return;
                    }

                    await TryRunHandler(handler, @event).ConfigureAwait(false);
                };
            }

            Func<Task>[] handlerActions =
                handlers.Cast<EventHandlerAsyncWrapper<TEvent>>()
                        .Select(CreateHandlerAction)
                        .ToArray();

            await _runner.RunAsync(handlerActions).ConfigureAwait(false);
        }

        private async Task EnqueueFromHandlersFactory<TEvent>(TEvent @event)
        {
            if (_handlersFactory == null)
            {
                return;
            }

            IEnumerable<IEventHandlerAsync<TEvent>> handlerInstances = _handlersFactory.AsyncHandlersFor<TEvent>();
            if (handlerInstances == null || !handlerInstances.Any())
            {
                return;
            }

            Func<Task> CreateHandlerAction(IEventHandlerAsync<TEvent> handler)
            {
                return async () => await TryRunHandler(handler, @event).ConfigureAwait(false);
            }

            Func<Task>[] handlerActions = handlerInstances.Select(CreateHandlerAction).ToArray();

            await _runner.RunAsync(handlerActions).ConfigureAwait(false);
        }

        private async Task TryRunHandler<TEvent>(IEventHandlerAsync<TEvent> handler, TEvent @event)
        {
            try
            {
                if (!await handler.ShouldHandleAsync(@event).ConfigureAwait(false))
                {
                    return;
                }

                await handler.HandleAsync(@event).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await TryReportError(exception, handler, @event).ConfigureAwait(false);
            }
        }

        private async Task TryReportError<TEvent>(Exception exception, IEventHandlerAsync<TEvent> handler, TEvent @event)
        {
            try
            {
                await handler.OnErrorAsync(exception, @event).ConfigureAwait(false);
            }
            catch
            {
                // yes, we mute exceptions here
            }
        }
    }
}
