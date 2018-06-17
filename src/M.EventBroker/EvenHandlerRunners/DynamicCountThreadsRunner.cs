using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Runs event handlers on background threads, creating and releasing threads depending on the load.
    /// </summary>
    public class DynamicCountThreadsRunner : IEventHandlerRunner
    {
        private readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(100);
        private readonly BlockingCollection<Action> _handlerActions = new BlockingCollection<Action>();
        private readonly Stack<ThreadInfo> _runningThreads = new Stack<ThreadInfo>();
        private readonly DynamicCountThreadsRunnerConfig _config;
        private bool _isRunning;

        /// <summary>
        /// Creates a new instance of the DynamicCountThreadsRunner class.
        /// </summary>
        /// <param name="config">Specifies configuration for handler threads.</param>
        public DynamicCountThreadsRunner(DynamicCountThreadsRunnerConfig config)
        {
            _config = config != null ? config : throw new ArgumentNullException($"Parameter {nameof(config)} should be initialized (value was: null)");
            _isRunning = true;

            Thread tracker = new Thread(Tracker);
            tracker.Start();

            StartWorkerThread();
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
                while (_runningThreads.Count > 0)
                {
                    var threadInfo = _runningThreads.Pop();
                    threadInfo.IsRunning = false;
                }

                Thread.Sleep(_timeout > _config.CheckInterval ? _timeout : _config.CheckInterval);
            }

            _handlerActions.Dispose();
        }

        private void Tracker()
        {
            int cyclesUnderTreshold = 0, cyclesOverTreshold = 0;
            while (_isRunning)
            {
                int waitingHandlersCount = _handlerActions.Count;
                bool isOverTreshold = waitingHandlersCount > _config.WaitingHandlersCountTreshold;
                if (isOverTreshold)
                {
                    cyclesOverTreshold++;
                    cyclesUnderTreshold = 0;
                    if (cyclesOverTreshold > _config.CyclesBeforeAddingThread)
                    {
                        cyclesOverTreshold = 0;
                        StartWorkerThread();
                    }
                }
                else
                {
                    cyclesUnderTreshold++;
                    cyclesOverTreshold = 0;
                    if (cyclesUnderTreshold > _config.CyclesBeforeReleasingThread)
                    {
                        cyclesUnderTreshold = 0;
                        ReleaseWorkerThread();
                    }
                }

                Thread.Sleep(_config.CheckInterval);
            }
        }

        private void StartWorkerThread()
        {
            if (_runningThreads.Count > _config.MaxThreadsCount)
            {
                return;
            }

            var workerThread = new Thread(Worker);
            var threadInfo = new ThreadInfo { IsRunning = true };
            _runningThreads.Push(threadInfo);
            workerThread.Start(threadInfo);
        }

        private void ReleaseWorkerThread()
        {
            if (_runningThreads.Count == 1)
            {
                return;
            }

            ThreadInfo threadInfo = _runningThreads.Pop();
            threadInfo.IsRunning = false;
        }

        private void Worker(object parameter)
        {
            var threadInfo = (ThreadInfo)parameter;
            while (_isRunning)
            {
                if (!threadInfo.IsRunning)
                {
                    return;
                }

                if (!_handlerActions.TryTake(out Action action, _timeout))
                {
                    continue;
                }

                action();
            }
        }

        private class ThreadInfo
        {
            public bool IsRunning { get; set; }
        }
    }
}