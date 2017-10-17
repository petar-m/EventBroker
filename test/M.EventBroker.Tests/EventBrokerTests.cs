using FakeItEasy;
using System;
using System.Threading;
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
             .WhenArgumentsMatch((c)=>c.Get<string>(0) == "event")
             .MustHaveHappened( Repeated.Exactly.Once);
        }

        [Fact]
        public void Subscribe_WithHandlerInstance_CreatesSubscribsion()
        {
            var broker = new EventBroker(3);
            var handler = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handler.ExecuteHandler(null))
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

            broker.Subscribe<string>(handler);
            broker.Unsubscribe<string>(handler);
            broker.Publish("event");

            Thread.Sleep(100);
            A.CallTo(() => handler.Handle(null))
             .WithAnyArguments()
             .MustNotHaveHappened();
        }

        public void Subscribe_WithIEventHandlerInstance_CreatesSubscribsion()
        {
            var broker = new EventBroker(3);
            // TODO: Mock event handler, assert called
            string @event = "event";
            string result = null;
            broker.Subscribe<string>(x => result = x);

            broker.Publish(@event);

            Assert.Equal(@event, result);
        }
    }
}
