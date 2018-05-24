using System;

namespace M.EventBroker
{
    public interface IEventHandlerRunner : IDisposable
    {
        void Run(params Action[] handlers);
    }
}