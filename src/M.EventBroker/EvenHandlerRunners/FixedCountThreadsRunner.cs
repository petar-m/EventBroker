﻿using System;
using System.Collections.Concurrent;
using System.Threading;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on fixed count background threads.
    /// </summary>
    public class FixedCountThreadsRunner : IEventHandlerRunner
    {
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);
        private readonly BlockingCollection<Action> _handlerActions = new BlockingCollection<Action>();
        private bool _isRunning;

        /// <summary>
        /// Creates a new instance of the FixedCountThreadsRunner class.
        /// </summary>
        /// <param name="workerThreadsCount">Specifies the count of threads to use for running event handlers. Threads are created and stared in the constructor.</param>
        public FixedCountThreadsRunner(int workerThreadsCount)
        {
            workerThreadsCount = workerThreadsCount > 0 ? workerThreadsCount : throw new ArgumentOutOfRangeException($"Parameter {nameof(workerThreadsCount)} should be positive integer (value was: {workerThreadsCount})");
            _isRunning = true;
            for (int i = 0; i < workerThreadsCount; i++)
            {
                Thread thread = new Thread(Worker);
                thread.Start();
            }
        }

        /// <summary>
        /// Runs events handlers on available background thread.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        public void Run(params Action[] handlers)
        {
            foreach (var handler in handlers)
            {
                _handlerActions.Add(handler);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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

                action();
            }
        }
    }
}