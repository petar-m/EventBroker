using System.Threading;
using FakeItEasy;
using Xunit;

namespace M.EventBroker.Tests
{
    public class EventBrokerTests
    {
        [Fact]
        public void Subscribe_WithDelegate_CreatesSubscribsion()
        {
            var broker = new EventBroker(3);
            var handler = A.Fake<IEventHandler<string>>();
            broker.Subscribe<string>(handler.Handle);

            broker.Publish("event");

            Thread.Sleep(100);
            A.CallTo(() => handler.Handle(null))
             .WhenArgumentsMatch((c) => c.Get<string>(0) == "event")
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Subscribe_WithHandlerInstance_CreatesSubscribsion()
        {
            var broker = new EventBroker(3);
            var handler = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handler.ShouldHandle(null))
             .WithAnyArguments()
             .Returns(true);
            broker.Subscribe(handler);

            broker.Publish("event");

            Thread.Sleep(100);
            A.CallTo(() => handler.Handle(null))
             .WhenArgumentsMatch((c) => c.Get<string>(0) == "event")
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Unsubscribe_WithDelegate_RemovesSubscribsion()
        {
            var broker = new EventBroker(3);
            var handler = A.Fake<IEventHandler<string>>();

            broker.Subscribe<string>(handler.Handle);
            broker.Unsubscribe<string>(handler.Handle);
            broker.Publish("event");

            Thread.Sleep(100);
            A.CallTo(() => handler.Handle(null))
             .WithAnyArguments()
             .MustNotHaveHappened();
        }

        [Fact]
        public void Unsubscribe_WithHandlerInstance_RemovesSubscribsion()
        {
            var broker = new EventBroker(3);
            var handler = A.Fake<IEventHandler<string>>();

            broker.Subscribe(handler);
            broker.Unsubscribe(handler);
            broker.Publish("event");

            Thread.Sleep(100);
            A.CallTo(() => handler.Handle(null))
             .WithAnyArguments()
             .MustNotHaveHappened();
        }
    }
}