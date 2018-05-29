﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using FakeItEasy;
using Xunit;

namespace M.EventBroker.Tests
{
    public class FireAndForgetRunnerTests
    {
        [Fact]
        public void EventBroker_DisposeWhileRunning_ExitsGracefully()
        {
            using (var broker = new EventBroker(new FireAndForgetRunner(1)))
            {
                broker.Subscribe<int>(x => Thread.Sleep(800));
                broker.Subscribe<int>(x => Thread.Sleep(800));

                broker.Publish(1);
                Thread.Sleep(100);
            }

            Thread.Sleep(2000);
            Assert.True(true);
        }

        [Fact]
        public void EventBroker_DisposeTwiceWhileRunning_ExitsGracefully()
        {
            using (var broker = new EventBroker(new FireAndForgetRunner(1)))
            {
                broker.Subscribe<int>(x => Thread.Sleep(800));
                broker.Subscribe<int>(x => Thread.Sleep(800));

                broker.Publish(1);
                Thread.Sleep(100);
                broker.Dispose();
            }

            Thread.Sleep(2000);
            Assert.True(true);
        }

        [Fact]
        public void Publish_WithMultipleSubscribersAndMultipleWorkers_HandlersAreCalledByDifferentThreads()
        {
            var broker = new EventBroker(new FireAndForgetRunner(2));
            int? thread1 = null;
            Action<int> handler1 = x => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };
            int? thread2 = null;
            Action<int> handler2 = x => { thread2 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };
            broker.Subscribe(handler1);
            broker.Subscribe(handler2);

            broker.Publish(1);

            Thread.Sleep(100);
            Assert.NotNull(thread1);
            Assert.NotNull(thread2);
            Assert.NotEqual(thread1, thread2);
        }

        [Fact]
        public void Publish_WithMultipleSubscribersAndSingleWorker_AllHandlersAreCalled()
        {
            var handler = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handler.ShouldHandle(A<string>.Ignored))
             .Returns(true);
            var eventHandlersFactory = new EventBrokerTests.EventHandlersFactory();
            eventHandlersFactory.Add(() => handler);
            var broker = new EventBroker(new FireAndForgetRunner(1), eventHandlersFactory);
            var stringHandler1 = A.Fake<IEventHandler<string>>();
            A.CallTo(() => stringHandler1.ShouldHandle(A<string>.Ignored))
             .Returns(true);
            broker.Subscribe(stringHandler1);
            var stringHandler2 = A.Fake<IEventHandler<string>>();
            broker.Subscribe<string>(stringHandler2.Handle);
            var intHandler = A.Fake<IEventHandler<int>>();
            broker.Subscribe(intHandler);

            broker.Publish("event");

            Thread.Sleep(100);
            A.CallTo(() => handler.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => stringHandler1.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => stringHandler2.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => intHandler.Handle(A<int>.Ignored))
             .MustNotHaveHappened();
        }
    }
}
