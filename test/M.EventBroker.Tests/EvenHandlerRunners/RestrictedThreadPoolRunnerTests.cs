using FakeItEasy;
using M.EventBroker.EvenHandlerRunners;
using System;
using System.Threading;
using Xunit;

namespace M.EventBroker.Tests.EvenHandlerRunners
{
    public class RestrictedThreadPoolRunnerTests
    {
        [Fact]
        public void Run_WithRestrictionOfTwoAndTwoActions_ActionsAreRunnedOnDifferentThreads()
        {
            // Arrange
            int? thread1 = null;
            Action action1 = () => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            int? thread2 = null;
            Action action2 = () => { thread2 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            var runner = new RestrictedThreadPoolRunner(2);

            // Act
            runner.Run(action1, action2);

            // Assert
            Thread.Sleep(100);

            Assert.NotNull(thread1);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread1);

            Assert.NotNull(thread2);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread2);

            Assert.NotEqual(thread1, thread2);
        }

        [Fact]
        public void Run_WithRestrictionOfOneAndMultipleAcrions_AllActionsAreRunned()
        {
            // Arrange
            var action1 = A.Fake<IAction>();
            A.CallTo(() => action1.Action()).Invokes(() => Thread.Sleep(50));

            var action2 = A.Fake<IAction>();
            A.CallTo(() => action2.Action()).Invokes(() => Thread.Sleep(50));

            var action3 = A.Fake<IAction>();
            A.CallTo(() => action3.Action()).Invokes(() => Thread.Sleep(50));

            var runner = new RestrictedThreadPoolRunner(1);

            // Act
            runner.Run(action1.Action, action2.Action, action3.Action);

            // Assert
            Thread.Sleep(1000);
            A.CallTo(() => action1.Action())
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => action2.Action())
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => action3.Action())
             .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Constructor_WithNegativeMaxConcurrentHandlers_ThrowsException()
        {
            Action constructor = () => new RestrictedThreadPoolRunner(-3);
            Assert.Throws<ArgumentOutOfRangeException>(constructor);
        }

        [Fact]
        public void Constructor_WithZeroMaxConcurrentHandlers_ThrowsException()
        {
            Action constructor = () => new RestrictedThreadPoolRunner(0);
            Assert.Throws<ArgumentOutOfRangeException>(constructor);
        }

        [Fact]
        public void Run_DisposeWhileRunning_ExitsGracefully()
        {
            // Arrange
            var action1 = A.Fake<IAction>();
            A.CallTo(() => action1.Action()).Invokes(() => Thread.Sleep(200));

            var action2 = A.Fake<IAction>();
            A.CallTo(() => action2.Action()).Invokes(() => Thread.Sleep(200));

            // Act
            using (var runner = new RestrictedThreadPoolRunner(1))
            {
                runner.Run(action1.Action, action2.Action);
            }

            // Assert
            Thread.Sleep(1000);

            A.CallTo(() => action1.Action())
            .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => action2.Action())
             .MustNotHaveHappened();
        }

        [Fact]
        public void Run_DisposeTwiceWhileRunning_ExitsGracefully()
        {
            // Arrange
            var action1 = A.Fake<IAction>();
            A.CallTo(() => action1.Action()).Invokes(() => Thread.Sleep(200));

            // Act
            using (var runner = new RestrictedThreadPoolRunner(1))
            {
                runner.Run(action1.Action);
                runner.Dispose();
            }

            // Assert
            Thread.Sleep(1000);

            A.CallTo(() => action1.Action())
                .MustNotHaveHappened();
        }
    }
}
