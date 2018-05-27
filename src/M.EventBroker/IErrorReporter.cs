using System;

namespace M.EventBroker
{
    /// <summary>Represents an action taken when exception is caught.</summary>
    public interface IErrorReporter
    {
        /// <summary>Performs an action when exception is caught.</summary>
        /// <param name="exception">The exception caught.</param>
        void Report(Exception exception);
    }
}

