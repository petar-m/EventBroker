using System;

namespace M.EventBroker.EvenHandlerRunners
{
    /// <summary>
    /// Specifies configuration parameters for DynamicCountThreadsRunner class.
    /// </summary>
    public class DynamicCountThreadsRunnerConfig
    {
        /// <summary>
        /// Creates a new instance of the DynamicCountThreadsRunnerConfig class.
        /// </summary>
        /// <param name="checkInterval">Specifies how often to check the treshold.</param>
        /// <param name="waitingHandlersCountTreshold">Specifies how many handlers waiting to be runned should trigger creation of new worker thread.</param>
        /// <param name="cyclesBeforeAddingThread">Specifies the number of consequent check cycles with exceeded treshold trigger creation of new worker thread.</param>
        /// <param name="cyclesBeforeReleasingThread">Specifies the number of consequent check cycles under treshold trigger releasing of worker thread.</param>
        /// <param name="maxThreadsCount">Specifies the maximum number of worker threads.</param>
        public DynamicCountThreadsRunnerConfig(
            TimeSpan checkInterval,
            int waitingHandlersCountTreshold,
            int cyclesBeforeAddingThread,
            int cyclesBeforeReleasingThread,
            int maxThreadsCount)
        {
            WaitingHandlersCountTreshold = waitingHandlersCountTreshold > 0 ? waitingHandlersCountTreshold : throw new ArgumentOutOfRangeException($"Parameter {nameof(waitingHandlersCountTreshold)} should be positive integer (value was: {waitingHandlersCountTreshold})");
            CheckInterval = checkInterval > TimeSpan.Zero ? checkInterval : throw new ArgumentOutOfRangeException($"Parameter {nameof(checkInterval)} should be positive TimeSpan (value was: {checkInterval})");
            MaxThreadsCount = maxThreadsCount > 0 ? maxThreadsCount : throw new ArgumentOutOfRangeException($"Parameter {nameof(maxThreadsCount)} should be positive integer (value was: {maxThreadsCount})");
            CyclesBeforeAddingThread = cyclesBeforeAddingThread > 0 ? cyclesBeforeAddingThread : throw new ArgumentOutOfRangeException($"Parameter {nameof(cyclesBeforeAddingThread)} should be positive integer (value was: {cyclesBeforeAddingThread})");
            CyclesBeforeReleasingThread = cyclesBeforeReleasingThread > 0 ? cyclesBeforeReleasingThread : throw new ArgumentOutOfRangeException($"Parameter {nameof(cyclesBeforeReleasingThread)} should be positive integer (value was: {cyclesBeforeReleasingThread})");
        }

        /// <summary>
        /// How often to check the treshold.
        /// </summary>
        public TimeSpan CheckInterval { get; }

        /// <summary>
        /// How many handlers waiting to be runned should trigger creation of new worker thread.
        /// </summary>
        public int WaitingHandlersCountTreshold { get; }

        /// <summary>
        /// Maximum number of worker threads.
        /// </summary>
        public int MaxThreadsCount { get; }

        /// <summary>
        /// Number of consequent check cycles under treshold trigger before releasing of worker thread.
        /// </summary>
        public int CyclesBeforeReleasingThread { get; }

        /// <summary>
        /// Number of consequent check cycles with exceeded treshold trigger creation of new worker thread.
        /// </summary>
        public int CyclesBeforeAddingThread { get; }

        /// <summary>
        /// Creates a new instance of the DynamicCountThreadsRunnerConfig class with default values:
        /// checkInterval: TimeSpan.FromMilliseconds(100)
        /// waitingHandlersCountTreshold: 2
        /// cyclesBeforeAddingThread: 2
        /// cyclesBeforeReleasingThread: 20
        /// maxThreadsCount: 5
        /// </summary>
        /// <returns>A new instance of the DynamicCountThreadsRunnerConfig class initialized with default values.</returns>
        public static DynamicCountThreadsRunnerConfig Default()
        {
            return new DynamicCountThreadsRunnerConfig(
                checkInterval: TimeSpan.FromMilliseconds(100),
                waitingHandlersCountTreshold: 2,
                cyclesBeforeAddingThread: 2,
                cyclesBeforeReleasingThread: 20,
                maxThreadsCount: 5);
        }
    }
}