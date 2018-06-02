using System;
using System.Threading;
using FakeItEasy;
using Xunit;

namespace M.EventBroker.Tests
{
    public class FixedCountThreadsRunnerTests
    {
        [Fact]
        public void Publish_WithMultipleSubscribersAndMultipleWorkers_HandlersAreRunnedOnDifferentThreads()
        {
            // Arrange
            int? thread1 = null;
            Action handler1 = () => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            int? thread2 = null;
            Action handler2 = () => { thread2 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            var runner = new FixedCountThreadsRunner(2);

            // Act
            runner.Run(handler1, handler2);

            // Assert
            Thread.Sleep(100);
            Assert.NotNull(thread1);
            Assert.NotNull(thread2);
            Assert.NotEqual(thread1, thread2);
        }

        [Fact]
        public void Publish_WithMultipleSubscribersAndSingleWorker_AllHandlersAreRunned()
        {
            // Arrange
            var handler1 = A.Fake<IAction>();
            var handler2 = A.Fake<IAction>();
            var handler3 = A.Fake<IAction>();

            var runner = new FixedCountThreadsRunner(1);

            // Act
            runner.Run(handler1.Action, handler2.Action, handler3.Action);

            // Assert
            Thread.Sleep(100);
            A.CallTo(() => handler1.Action())
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handler2.Action())
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => handler3.Action())
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void ErrorReporter_OnException_ExceptionIsReported()
        {
            // Arrange
            var exception = new InvalidOperationException("exception during execution");
            Action handler = () => throw exception;

            var errorReporterMock = A.Fake<IErrorReporter>();

            var runner = new FixedCountThreadsRunner(1, errorReporterMock);

            // Act 
            runner.Run(handler);

            // Assert
            Thread.Sleep(50);
            A.CallTo(() => errorReporterMock.Report(exception))
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void NoErrorReporter_OnException_ExceptionIsSwallowed()
        {
            // Arrange
            Action handler = () => throw new Exception();
            var runner = new FixedCountThreadsRunner(1);

            // Act 
            runner.Run(handler);

            // Assert
            Thread.Sleep(50);
            Assert.True(true);
        }

        [Fact]
        public void Constructor_WithNegativeThreadsCount_ThrowsException()
        {
            Action constructor = () => new FixedCountThreadsRunner(-3);
            Assert.Throws<ArgumentOutOfRangeException>(constructor);
        }

        [Fact]
        public void Constructor_WithZeroThreadsCount_ThrowsException()
        {
            Action constructor = () => new FixedCountThreadsRunner(0);
            Assert.Throws<ArgumentOutOfRangeException>(constructor);
        }

        [Fact]
        public void EventBroker_DisposeWhileRunning_ExitsGracefully()
        {
            using (var broker = new EventBroker(new FixedCountThreadsRunner(1)))
            {
                broker.Subscribe<int>(_ => Thread.Sleep(200));
                broker.Subscribe<int>(_ => Thread.Sleep(200));

                broker.Publish(1);
                Thread.Sleep(100);
            }

            Thread.Sleep(700);
            Assert.True(true);
        }

        [Fact]
        public void EventBroker_DisposeTwiceWhileRunning_ExitsGracefully()
        {
            using (var broker = new EventBroker(new FixedCountThreadsRunner(1)))
            {
                broker.Subscribe<int>(_ => Thread.Sleep(200));
                broker.Subscribe<int>(_ => Thread.Sleep(200));

                broker.Publish(1);
                Thread.Sleep(50);
                broker.Dispose();
            }

            Thread.Sleep(600);
            Assert.True(true);
        }

        public interface IAction
        {
            void Action();
        }
    }
}
