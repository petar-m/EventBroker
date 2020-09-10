using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace M.EventBroker.Async.Tests
{
    public class EventBrokerAsyncTests
    {
        [Fact]
        public async Task Subscribe_WithDelegate_CreatesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            var broker = new EventBrokerAsync(handlerRunnerMock);

            var handler = A.Fake<IEventHandlerAsync<string>>();

            // Act
            broker.Subscribe<string>(handler.HandleAsync);
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Subscribe_WithHandlerInstance_CreatesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            var broker = new EventBrokerAsync(handlerRunnerMock);

            var handler = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handler.ShouldHandleAsync(null))
             .WithAnyArguments()
             .Returns(true);

            // Act
            broker.Subscribe(handler);
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Unsubscribe_WithDelegate_RemovesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            var broker = new EventBrokerAsync(handlerRunnerMock);

            var handler = A.Fake<IEventHandlerAsync<string>>();

            // Act
            broker.Subscribe<string>(handler.HandleAsync);
            broker.Unsubscribe<string>(handler.HandleAsync);
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Unsubscribe_WithHandlerInstance_RemovesSubscribsion()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            var broker = new EventBrokerAsync(handlerRunnerMock);

            var handler = A.Fake<IEventHandlerAsync<string>>();

            // Act
            broker.Subscribe(handler);
            broker.Unsubscribe(handler);
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Publish_WithHandlersFactory_HandlerIsRunned()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock);

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock, eventHandlersFactory);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.HandleAsync("event"))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Publish_WithHandlersFactory_FilterIsRespected()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(false);

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock);

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock, eventHandlersFactory);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.HandleAsync("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Publish_WithHandlersFactoryReturningNull_NothingHappens()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            var eventHandlersFactory = new EventHandlersFactory();

            var broker = new EventBrokerAsync(handlerRunnerMock, eventHandlersFactory);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Publish_WithHandlerInstanceSubscription_FilterIsRespected()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(false);

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe(handlerMock);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.HandleAsync("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Publish_WithDelegateSubscription_FilterIsRespected()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(false);

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.HandleAsync, handlerMock.ShouldHandleAsync);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.HandleAsync("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Publish_WithMultipleSubscribers_AllHandlersAreRunned()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First()).ToList().ForEach(a => a.Invoke()));

            var handlerMock1 = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handlerMock1.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock1);

            var broker = new EventBrokerAsync(handlerRunnerMock, eventHandlersFactory);

            var handlerMock2 = A.Fake<IEventHandlerAsync<string>>();
            A.CallTo(() => handlerMock2.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);
            broker.Subscribe(handlerMock2);

            var handlerMock3 = A.Fake<IEventHandlerAsync<string>>();
            broker.Subscribe<string>(handlerMock3.HandleAsync);

            var handlerMock4 = A.Fake<IEventHandlerAsync<int>>();
            broker.Subscribe(handlerMock4);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerMock1.HandleAsync("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock2.HandleAsync("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock3.HandleAsync("event"))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock4.HandleAsync(A<int>.Ignored))
             .MustNotHaveHappened();
        }

        [Fact]
        public void EventBroker_Dispose_DisposesIHandlerRunner()
        {
            // Arrange
            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            var broker = new EventBrokerAsync(handlerRunnerMock);

            // Act
            broker.Dispose();

            // Assert
            A.CallTo(() => handlerRunnerMock.Dispose())
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Publish_WithDelegateSubscriptionUnsubscribedWhileWaiting_HandlerIsNotRunned()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();
            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x =>
             {
                 Task.Factory.StartNew(() =>
                 {
                     Thread.Sleep(500);
                     ((Func<Task>[])x.Arguments.First())[0].Invoke();
                 });
             });

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.HandleAsync);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);
            broker.Unsubscribe<string>(handlerMock.HandleAsync);

            // Assert
            Thread.Sleep(500);
            A.CallTo(() => handlerRunnerMock.RunAsync(A<Func<Task>>.Ignored))
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handlerMock.HandleAsync("event"))
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task SubscribedWithOnErrorDelegate_ExceptionThrownDuringHandling_OnExceptionIsCalled()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.HandleAsync, handlerMock.ShouldHandleAsync, handlerMock.OnErrorAsync);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerMock.OnErrorAsync(A<Exception>.Ignored, "event"))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task SubscribedWithoutOnErrorDelegate_ExceptionThrownDuringHandling_ExceptionIsMuted()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.HandleAsync, handlerMock.ShouldHandleAsync, null);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task SubscribedWithOnErrorDelegate_ExceptionThrownDuringOnError_ExceptionIsMuted()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            A.CallTo(() => handlerMock.OnErrorAsync(A<Exception>.Ignored, A<string>.Ignored))
             .Throws<Exception>();

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe<string>(handlerMock.HandleAsync, handlerMock.ShouldHandleAsync, handlerMock.OnErrorAsync);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task SubscribedWithHandlerInstancee_ExceptionThrownDuringHandling_OnExceptionIsCalled()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe(handlerMock);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerMock.OnErrorAsync(A<Exception>.Ignored, "event"))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task SubscribedWithHandlerInstance_ExceptionThrownDuringOnError_ExceptionIsMuted()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            A.CallTo(() => handlerMock.OnErrorAsync(A<Exception>.Ignored, A<string>.Ignored))
             .Throws<Exception>();

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock);
            broker.Subscribe(handlerMock);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public async Task SubscribedWithHandlersFactory_ExceptionThrownDuringHandling_OnExceptionIsCalled()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock);

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock, eventHandlersFactory);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            A.CallTo(() => handlerMock.OnErrorAsync(A<Exception>.Ignored, "event"))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task SubscribedWithHandlersFactory_ExceptionThrownDuringOnError_ExceptionIsMuted()
        {
            // Arrange
            var handlerMock = A.Fake<IEventHandlerAsync<string>>();

            A.CallTo(() => handlerMock.ShouldHandleAsync(A<string>.Ignored))
             .Returns(true);

            A.CallTo(() => handlerMock.HandleAsync(A<string>.Ignored))
             .Throws<Exception>();

            A.CallTo(() => handlerMock.OnErrorAsync(A<Exception>.Ignored, A<string>.Ignored))
             .Throws<Exception>();

            var eventHandlersFactory = new EventHandlersFactory();
            eventHandlersFactory.Add(() => handlerMock);

            var handlerRunnerMock = A.Fake<IEventHandlerRunnerAsync>();

            A.CallTo(() => handlerRunnerMock.RunAsync(null))
             .WithAnyArguments()
             .Invokes(x => ((Func<Task>[])x.Arguments.First())[0].Invoke());

            var broker = new EventBrokerAsync(handlerRunnerMock, eventHandlersFactory);

            // Act
            await broker.PublishAsync("event").ConfigureAwait(false);

            // Assert
            Assert.True(true);
        }

        public class EventHandlersFactory : IEventHandlerAsyncFactory
        {
            private readonly List<(Type type, object handler)> _handlers = new List<(Type, object)>();

            public void Add<T>(Func<IEventHandlerAsync<T>> handlerProvider)
            {
                _handlers.Add((typeof(T), handlerProvider));
            }

            public IEnumerable<IEventHandlerAsync<T>> AsyncHandlersFor<T>()
            {
                return _handlers.Count > 0
                        ? _handlers.Where(x => x.type == typeof(T))
                                 .Select(x => ((Func<IEventHandlerAsync<T>>)x.handler)())
                                 .ToArray()
                        : null;
            }
        }
    }
}
