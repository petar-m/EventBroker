namespace M.EventBroker
{
    public interface IEventHandler<TEvent>
    {
        void Handle(TEvent @event);

        bool ShouldHandle(TEvent @event);
    }
}