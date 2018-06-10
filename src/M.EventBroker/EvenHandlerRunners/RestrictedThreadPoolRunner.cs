using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on a ThreadPool threads, restricting the running handlers to a given number.
    /// </summary>
    public class RestrictedThreadPoolRunner : IEventHandlerRunner
    {
        private object locker = new object();
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);
        private readonly BlockingCollection<Action> _handlerActions = new BlockingCollection<Action>();
        private int _currentlyRunning = 0;

        private bool _isRunning;
        private readonly int _maxConcurrentHandlers;

        /// <summary>
        /// Creates a new instance of the RestrictedThreadPoolRunner class.
        /// </summary>
        /// <param name="maxConcurrentHandlers">Specifies the maximum number of event handlers running concurrently.</param>
        public RestrictedThreadPoolRunner(int maxConcurrentHandlers)
        {
            _maxConcurrentHandlers = maxConcurrentHandlers > 0 ? maxConcurrentHandlers : throw new ArgumentOutOfRangeException($"Parameter {nameof(maxConcurrentHandlers)} should be positive integer (value was: {maxConcurrentHandlers})");
            _isRunning = true;
            Thread thread = new Thread(Worker);
            thread.Start();
        }

        /// <summary>
        /// Runs event handlers on a ThreadPool threads.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        public void Run(params Action[] handlers)
        {
            foreach (Action handler in handlers)
            {
                var handler1 = handler;
                _handlerActions.Add(handler1);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_isRunning)
            {
                _isRunning = false;
                Thread.Sleep(_timeout);
            }

            _handlerActions.Dispose();
        }

        private void Worker()
        {
            while (_isRunning)
            {
                if (!_handlerActions.TryTake(out Action action, _timeout))
                {
                    continue;
                }

                while (_isRunning)
                {
                    lock (locker)
                    {
                        if(_currentlyRunning < _maxConcurrentHandlers)
                        {
                            _currentlyRunning++;
                            Task.Run(RunHandler(action));
                            break;
                        }
                    }

                    Thread.Sleep(TimeSpan.FromMilliseconds(50));
                }
            }
        }

        private Action RunHandler(Action handler)
        {
            return () =>
            {
                handler();

                lock (locker)
                {
                    _currentlyRunning--;
                }
            };
        }
    }
}
