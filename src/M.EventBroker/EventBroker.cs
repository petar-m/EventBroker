using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace M.EventBroker
{
    /// <summary>
    /// Manages event subscriptions and invoking of event handlers.
    /// </summary>
    public class EventBroker : IEventBroker
    {
        private ConcurrentDictionary<Type, List<object>> subscribers = new ConcurrentDictionary<Type, List<object>>();
        private BlockingCollection<Action> handlerActions = new BlockingCollection<Action>();
        private bool isRunning;
        private readonly Action<Exception> errorReporter;
        private readonly Func<Type, IEnumerable<object>> handlersFactory;

        /// <summary>
        /// Creates a new instance of the EventBroker class.
        /// </summary>
        /// <param name="workerThreadsCount">Determines how many threads to use for calling event handlers.</param>
        /// <param name="errorReporter">A delegate to be called when exception is thrown from handler.</param>
        /// <param name="handlersFactory">A delegate providing event handlers for event of givent type.</param>
        public EventBroker(
            int workerThreadsCount,
            Action<Exception> errorReporter = null,
            Func<Type, IEnumerable<object>> handlersFactory = null)
        {
            isRunning = true;
            for (int i = 0; i < workerThreadsCount; i++)
            {
                Thread thread = new Thread(new ThreadStart(Worker));
                thread.Start();
            }

            this.errorReporter = errorReporter;
            this.handlersFactory = handlersFactory;
        }

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">A delegate that will be invoked when event is published.</param>
        /// <param name="filter">A delegate used to perform filtering of events before invoking the handler.</param>
        public void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter = null)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerWrapper<TEvent>(handler, filter));
        }

        /// <summary>
        /// Adds subscription for events of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">An instance of IEventHandler&lt;TEvent&gt; which Handle method will be invoked when event is published.</param>
        public void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
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
        public void Dispose()
        {
            if (isRunning)
            {
                isRunning = false;
                Thread.Sleep(1000);
            }

            handlerActions.Dispose();
        }

        private void Unsubscribe<TEvent>(Func<EventHandlerWrapper<TEvent>, bool> handlerPredicate)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            var targetHandlers = handlers.Cast<EventHandlerWrapper<TEvent>>().ToArray();
            foreach (var handlerAction in targetHandlers.Where(x => handlerPredicate(x)))
            {
                handlers.Remove(handlerAction);
                handlerAction.IsSubscribed = false;
            }
        }

        private void EnqueueSubscribers<TEvent>(TEvent @event)
        {
            bool hasSubscribers = subscribers.TryGetValue(typeof(TEvent), out List<object> handlers);
            if (!hasSubscribers)
            {
                return;
            }

            foreach (var handler in handlers.Cast<EventHandlerWrapper<TEvent>>().ToArray())
            {
                var handler1 = handler;
                handlerActions.Add(() =>
                {
                    if (!handler1.IsSubscribed)
                    {
                        return;
                    }

                    if (!handler1.ShouldHandle(@event))
                    {
                        return;
                    }

                    handler1.Handle(@event);
                });
            }
        }

        private void EnqueueFromHandlersFactory<TEvent>(TEvent @event)
        {
            if (handlersFactory == null)
            {
                return;
            }

            var handlerInstances = handlersFactory(typeof(TEvent));
            if (handlerInstances == null)
            {
                return;
            }
            foreach (var handler in handlerInstances.Cast<IEventHandler<TEvent>>().ToArray())
            {
                var handler1 = handler;
                handlerActions.Add(() =>
                {
                    if (!handler1.ShouldHandle(@event))
                    {
                        return;
                    }

                    handler1.Handle(@event);
                });
            }
        }

        private void Worker()
        {
            int timeout = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
            while (isRunning)
            {
                if (!handlerActions.TryTake(out Action action, timeout))
                {
                    continue;
                }

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    errorReporter?.Invoke(ex);
                }
            }
        }
    }
}