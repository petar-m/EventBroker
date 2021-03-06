﻿using System;

namespace M.EventBroker
{
    /// <summary>
    /// Represents runner of event handlers.
    /// </summary>
    public interface IEventHandlerRunner : IDisposable
    {
        /// <summary>
        /// Runs event handlers.
        /// </summary>
        /// <param name="handlers">The event handlers to run.</param>
        void Run(params Action[] handlers);
    }
}
