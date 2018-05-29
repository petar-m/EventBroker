using System;
using System.Collections.Concurrent;
using System.Threading;

namespace M.EventBroker
{
    public class FireAndForgetRunner : IEventHandlerRunner
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);
        private readonly BlockingCollection<Action> _handlerActions = new BlockingCollection<Action>();
        private readonly IErrorReporter _errorReporter;
        private bool _isRunning;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="workerThreadsCount">Determines how many threads to use for calling event handlers.</param>
        public FireAndForgetRunner(int workerThreadsCount, IErrorReporter errorReporter = null)
        {
            _errorReporter = errorReporter;
            _isRunning = true;
            for (int i = 0; i < workerThreadsCount; i++)
            {
                Thread thread = new Thread(Worker);
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

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _errorReporter?.Report(ex);
                }
            }
        }
    }
}