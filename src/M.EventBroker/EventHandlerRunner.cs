using System;
using System.Collections.Concurrent;
using System.Threading;

namespace M.EventBroker
{
    public class EventHandlerRunner : IEventHandlerRunner
    {
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(1);
        private readonly BlockingCollection<Action> _handlerActions = new BlockingCollection<Action>();
        private bool _isRunning;
        private readonly Action<Exception> _errorReporter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workerThreadsCount">Determines how many threads to use for calling event handlers.</param>
        public EventHandlerRunner(int workerThreadsCount, Action<Exception> errorReporter = null)
        {
            _errorReporter = errorReporter;
            _isRunning = true;
            for (int i = 0; i < workerThreadsCount; i++)
            {
                Thread thread = new Thread(new ThreadStart(Worker));
                thread.Start();
            }
        }

        public void Run(params Action[] handlers)
        {
            foreach (var handler in handlers)
            {
                _handlerActions.Add(handler);
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            if (_isRunning)
            {
                _isRunning = false;
                Thread.Sleep(timeout);
            }

            _handlerActions.Dispose();
        }

        private void Worker()
        {
            while (_isRunning)
            {
                if (!_handlerActions.TryTake(out Action action, timeout))
                {
                    continue;
                }

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _errorReporter?.Invoke(ex);
                }
            }
        }
    }
}