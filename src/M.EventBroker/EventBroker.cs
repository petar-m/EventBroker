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
            handlers.Add(new InternalEventHandler<TEvent>(new DelegateEventHandler<TEvent>(handler, filter)));
        }

        public void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            handlers.Add(new InternalEventHandler<TEvent>(handler));
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            InternalEventHandler<TEvent>[] delegateHandlers = handlers.OfType<InternalEventHandler<TEvent>>().Where(x => ((DelegateEventHandler<TEvent>)x.EventHandler).Handler == handler).ToArray();
            foreach (var handlerAction in delegateHandlers)
            {
                handlers.Remove(handlerAction);
                handlerAction.Unsubscribed = true;
            }
        }

        public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler)
        {
            var handlers = subscribers.GetOrAdd(typeof(TEvent), _ => new List<object>());
            foreach (var handlerAction in handlers.OfType<InternalEventHandler<TEvent>>().Where(x => x.EventHandler == handler).ToArray())
            {
                handlers.Remove(handlerAction);
                handlerAction.Unsubscribed = true;
            }
        }

        public void Publish<TEvent>(TEvent @event)
        {
            EnqueueSubscribedHandlers(@event);
            EnqueueActivatedHandlers(@event);
        }

        public void Dispose()
        {
            if (isRunning)
            {
                isRunning = false;
                // TODO: ?
                Thread.Sleep(1000);
            }
        }

        private void EnqueueSubscribedHandlers<TEvent>(TEvent @event)
        {
            bool hasSubscribers = subscribers.TryGetValue(typeof(TEvent), out List<object> handlers);
            if (!hasSubscribers)
            {
                return;
            }

            foreach (var handler in handlers.Cast<IEventHandler<TEvent>>().ToArray())
            {
                var handler1 = handler;
                handlerActions.Add(() =>
                {
                    var status = handler1 as IHandlerStatus;
                    if (status != null && status.Unsubscribed)
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

        private void EnqueueActivatedHandlers<TEvent>(TEvent @event)
        {
            if (handlersFactory == null)
            {
                return;
            }

            var handlerInstances = handlersFactory(typeof(TEvent));
            foreach (var handler in handlerInstances.Cast<IEventHandler<TEvent>>())
            {
                var handler1 = handler;
                handlerActions.Add(() =>
                {
                    if (handler1.ShouldHandle(@event))
                    {
                        handler1.Handle(@event);
                    }
                });
            }
        }

        private void Worker()
        {
            int timeout = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
            while (isRunning)
            {
                if (handlerActions.TryTake(out Action action, timeout))
                {
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
}