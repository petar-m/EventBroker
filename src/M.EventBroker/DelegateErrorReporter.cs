using System;

namespace M.EventBroker
{
    /// <summary>
    /// An adapter used to represent delegate as IErrorReporter
    /// </summary>
    public class DelegateErrorReporter : IErrorReporter
    {
        private readonly Action<Exception> _errorReporter;

        /// <summary> Creates a new instance of the DelegateErrorReporter class. </summary>
        /// <param name="errorReporter">An action that will be called wthi the caught exception.</param>
        public DelegateErrorReporter(Action<Exception> errorReporter)
        {
            _errorReporter = errorReporter;
        }

        /// <summary>Performs an action when exception is caught.</summary>
        /// <param name="exception">The exception caught.</param>
        public void Report(Exception exception) => _errorReporter(exception);
    }
}

