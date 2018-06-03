using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace M.EventBroker
{
    /// <summary>
    /// Manages event subscriptions and invoking of event handlers.
    /// </summary>
    public class EventBroker : IEventBroker
    {
        private readonly ConcurrentDictionary<Type, List<object>> _subscribers = new ConcurrentDictionary<Type, List<object>>();
        private readonly IEventHandlerFactory _handlersFactory;
        private readonly IEventHandlerRunner _runner;

        /// <summary>
        /// Creates a new instance of the EventBroker class.
        /// </summary>
        /// <param name="runner"></param>
        /// <param name="handlersFactory">A delegate providing event handlers for event of givent type.</param>
        public EventBroker(IEventHandlerRunner runner, IEventHandlerFactory handlersFactory = null)
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
        public void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter = null, Action<Exception, TEvent> onError = null)
        {
            var handlers = _subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerWrapper<TEvent>(handler, filter, onError));
        }

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; which Handle method will be invoked when event is published.</param>
        public void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            var handlers = _subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerWrapper<TEvent>(handler));
        }

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate to remove form subscribers.</param>
        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            Unsubscribe<TEvent>(x => x.IsWrapping(handler));
        }

        /// <summary>
        /// Removes subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; to remove form subscribers.</param>
        public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            Unsubscribe<TEvent>(x => x.IsWrapping(handler));
        }

        /// <summary>
        /// Publishes an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="event">An <typeparamref name="TEvent"/> instance to be passed to all handlers of the event. All handlers will be invoked asynchronously.</param>
        public void Publish<TEvent>(TEvent @event)
        {
            EnqueueSubscribers(@event);
            EnqueueFromHandlersFactory(@event);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the EventBroker class.
        /// </summary>
        public void Dispose() => _runner.Dispose();

        private void Unsubscribe<TEvent>(Func<EventHandlerWrapper<TEvent>, bool> handlerPredicate)
        {
            var handlers = _subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            var targetHandlers = handlers.Cast<EventHandlerWrapper<TEvent>>().ToArray();
            foreach (var handlerAction in targetHandlers.Where(x => handlerPredicate(x)))
            {
                handlers.Remove(handlerAction);
                handlerAction.IsSubscribed = false;
            }
        }

        private void EnqueueSubscribers<TEvent>(TEvent @event)
        {
            bool hasSubscribers = _subscribers.TryGetValue(typeof(TEvent), out List<object> handlers);
            if (!hasSubscribers)
            {
                return;
            }

            Action CreateHandlerAction(EventHandlerWrapper<TEvent> handler)
            {
                return () =>
                {
                    if (!handler.IsSubscribed)
                    {
                        return;
                    }

                    TryRunHandler(handler, @event);
                };
            }

            Action[] handlerActions =
                handlers.Cast<EventHandlerWrapper<TEvent>>()
                        .Select(CreateHandlerAction)
                        .ToArray();

            _runner.Run(handlerActions);
        }

        private void EnqueueFromHandlersFactory<TEvent>(TEvent @event)
        {
            if (_handlersFactory == null)
            {
                return;
            }

            IEnumerable<IEventHandler<TEvent>> handlerInstances = _handlersFactory.HandlersFor<TEvent>();
            if (handlerInstances == null)
            {
                return;
            }

            Action CreateHandlerAction(IEventHandler<TEvent> handler)
            {
                return () => TryRunHandler(handler, @event);
            }

            Action[] handlerActions = handlerInstances.Select(CreateHandlerAction).ToArray();

            _runner.Run(handlerActions);
        }

        private void TryRunHandler<TEvent>(IEventHandler<TEvent> handler, TEvent @event)
        {
            try
            {
                if (!handler.ShouldHandle(@event))
                {
                    return;
                }

                handler.Handle(@event);
            }
            catch (Exception exception)
            {
                TryReportError(exception, handler, @event);
            }
        }

        private void TryReportError<TEvent>(Exception exception, IEventHandler<TEvent> handler, TEvent @event)
        {
            try
            {
                handler.OnError(exception, @event);
            }
            catch
            {
                // yes, we mute exceptions here
            }
        }
    }
}