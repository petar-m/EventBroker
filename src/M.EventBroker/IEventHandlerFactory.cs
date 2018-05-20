using System.Collections.Generic;

namespace M.EventBroker
{
    public interface IEventHandlerFactory
    {
        IEnumerable<IEventHandler<TEvent>> HandlersFor<TEvent>();
    }
}