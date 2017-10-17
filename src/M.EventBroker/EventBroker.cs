using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace M.EventBroker
{
    public class EventBroker : IEventBroker
    {
        private ConcurrentDictionary<Type, List<object>> subscribers = new ConcurrentDictionary<Type, List<object>>();
        private BlockingCollection<Action> handlerActions = new BlockingCollection<Action>();
        private bool isRunning;
        private readonly Action<Exception> errorReporter;
        private readonly Func<Type, IEnumerable<object>> handlersFactory;

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

        public void Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter = null)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerWrapper<TEvent>(handler, filter));
        }

        public void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new EventHandlerWrapper<TEvent>(handler));
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            Unsubscribe<TEvent>(x => x.IsWrapping(handler));
        }

        public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            Unsubscribe<TEvent>(x => x.IsWrapping(handler));
        }

        public void Publish<TEvent>(TEvent @event)
        {
            EnqueueSubscribers(@event);
            EnqueueFromHandlersFactory(@event);
        }

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