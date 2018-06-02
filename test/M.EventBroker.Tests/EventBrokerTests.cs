﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace M.EventBroker.Tests
{
    public class EventBrokerTests
    {
        [Fact]
        public void Subscribe_WithDelegate_CreatesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            var broker = new EventBroker(handlerRunnerMock);

            var handler = A.Fake<IEventHandler<string>>();

            // Act
            broker.Subscribe<string>(handler.Handle);
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Subscribe_WithHandlerInstance_CreatesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            var broker = new EventBroker(handlerRunnerMock);

            var handler = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handler.ShouldHandle(null))
             .WithAnyArguments()
             .Returns(true);

            // Act
            broker.Subscribe(handler);
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Unsubscribe_WithDelegate_RemovesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            var broker = new EventBroker(handlerRunnerMock);

            var handler = A.Fake<IEventHandler<string>>();

            // Act
            broker.Subscribe<string>(handler.Handle);
            broker.Unsubscribe<string>(handler.Handle);
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public void Unsubscribe_WithHandlerInstance_RemovesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            var broker = new EventBroker(handlerRunnerMock);

            var handler = A.Fake<IEventHandler<string>>();

            // Act
            broker.Subscribe(handler);
            broker.Unsubscribe(handler);
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public void Publish_WithHandlersFactory_HandlerIsRunned()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handlerMock.ShouldHandle(A<string>.Ignored))
             .Returns(true);

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock);

            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            A.CallTo(() => handlerRunnerMock.Run(null))
             .WithAnyArguments()
             .Invokes(x => ((Action[])x.Arguments.First()).First().Invoke());

            var broker = new EventBroker(handlerRunnerMock, eventHandlersFactory);

            // Act
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Publish_WithHandlersFactory_FilterIsRespected()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handlerMock.ShouldHandle(A<string>.Ignored))
             .Returns(false);

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock);

            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            A.CallTo(() => handlerRunnerMock.Run(null))
             .WithAnyArguments()
             .Invokes(x => ((Action[])x.Arguments.First()).First().Invoke());

            var broker = new EventBroker(handlerRunnerMock, eventHandlersFactory);

            // Act
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.Handle("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public void Publish_WithHandlersFactoryReturningNull_NothingHappens()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            var eventHandlersFactory = new EventHandlersFactory();

            var broker = new EventBroker(handlerRunnerMock, eventHandlersFactory);

            // Act
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public void Publish_WithHandlerInstanceSubscription_FilterIsRespected()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handlerMock.ShouldHandle(A<string>.Ignored))
             .Returns(false);

            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            A.CallTo(() => handlerRunnerMock.Run(null))
             .WithAnyArguments()
             .Invokes(x => ((Action[])x.Arguments.First()).First().Invoke());

            var broker = new EventBroker(handlerRunnerMock);
            broker.Subscribe(handlerMock);

            // Act
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.Handle("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public void Publish_WithDelegateSubscription_FilterIsRespected()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handlerMock.ShouldHandle(A<string>.Ignored))
             .Returns(false);

            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            A.CallTo(() => handlerRunnerMock.Run(null))
             .WithAnyArguments()
             .Invokes(x => ((Action[])x.Arguments.First()).First().Invoke());

            var broker = new EventBroker(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.Handle, handlerMock.ShouldHandle);

            // Act
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.Handle("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public void Publish_WithMultipleSubscribers_AllHandlersAreRunned()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            A.CallTo(() => handlerRunnerMock.Run(null))
             .WithAnyArguments()
             .Invokes(x => ((Action[])x.Arguments.First()).ToList().ForEach(a => a.Invoke()));

            var handlerMock1 = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handlerMock1.ShouldHandle(A<string>.Ignored))
             .Returns(true);

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock1);

            var broker = new EventBroker(handlerRunnerMock, eventHandlersFactory);

            var handlerMock2 = A.Fake<IEventHandler<string>>();
            A.CallTo(() => handlerMock2.ShouldHandle(A<string>.Ignored))
             .Returns(true);
            broker.Subscribe(handlerMock2);

            var handlerMock3 = A.Fake<IEventHandler<string>>();
            broker.Subscribe<string>(handlerMock3.Handle);

            var handlerMock4 = A.Fake<IEventHandler<int>>();
            broker.Subscribe(handlerMock4);

            // Act
            broker.Publish("event");

            // Assert
            A.CallTo(() => handlerMock1.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock2.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock3.Handle("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock4.Handle(A<int>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public void EventBroker_Dispose_DisposesIHandlerRunner()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            var broker = new EventBroker(handlerRunnerMock);
                      
            // Act
            broker.Dispose();

            // Assert
            A.CallTo(() => handlerRunnerMock.Dispose())
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Publish_WithDelegateSubscriptionUnsubscribedWhileWaiting_HandlerIsNotRunned()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandler<string>>();
            
            var handlerRunnerMock = A.Fake<IEventHandlerRunner>();
            A.CallTo(() => handlerRunnerMock.Run(null))
             .WithAnyArguments()
             .Invokes(x =>
             {
                 Task.Factory.StartNew(() =>
                 {
                     Thread.Sleep(500);
                     ((Action[])x.Arguments.First()).First().Invoke();
                 });
             });

            var broker = new EventBroker(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.Handle);

            // Act
            broker.Publish("event");
            broker.Unsubscribe<string>(handlerMock.Handle);

            // Assert
            Thread.Sleep(500);
            A.CallTo(() => handlerRunnerMock.Run(A<Action>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.Handle("event"))
             .MustNotHaveHappened();
        }

        public class EventHandlersFactory : IEventHandlerFactory
        {
            readonly List<(Type type, object handler)> _handlers = new List<(Type, object)>();

            public void Add<T>(Func<IEventHandler<T>> handlerProvider)
            {
                _handlers.Add((typeof(T), handlerProvider));
            }

            public IEnumerable<IEventHandler<T>> HandlersFor<T>()
            {
                return _handlers.Any()
                        ? _handlers.Where(x => x.type == typeof(T))
                                 .Select(x => ((Func<IEventHandler<T>>)x.handler)())
                                 .ToArray()
                        : null;
            }
        }
    }
}