namespace M.EventBroker
{
    public interface IEventHandler<TEvent>
    {
        void Handle(TEvent @event);
        
        // TODO: rename to be a question.
        bool ExecuteHandler(TEvent @event);
    }
}