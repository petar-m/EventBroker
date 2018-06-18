using FakeItEasy;
using M.EventBroker.EvenHandlerRunners;
using System;
using System.Threading;
using Xunit;

namespace M.EventBroker.Tests.EvenHandlerRunners
{
    public class DynamicCountThreadsRunnerTests
    {
        [Fact]
        public void Constructor_WithNullArgument_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicCountThreadsRunner(null));
        }

        [Fact]
        public void Run_ActionWithOneWorker_RunnedOnDifferentThread()
        {
            // Arrange
            int? thread1 = null;
            Action action1 = () => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            var runnerConfig = new DynamicCountThreadsRunnerConfig(TimeSpan.FromMilliseconds(100), 2, 2, 5, 1);

            var runner = new DynamicCountThreadsRunner(runnerConfig);

            // Act
            runner.Run(action1);

            // Assert
            Thread.Sleep(100);

            Assert.NotNull(thread1);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread1);
            Assert.Equal(1, runner.WorkerThreadsCount);
        }

        [Fact]
        public void Run_OverTreshold_NewThreadCreated()
        {
            // Arrange
            int? thread1 = null;
            Action action1 = () => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(300); };

            int? thread2 = null;
            Action action2 = () => { thread2 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            Action action3 = () => Thread.Sleep(30);

            var runnerConfig = new DynamicCountThreadsRunnerConfig(TimeSpan.FromMilliseconds(50), 2, 2, 5, 2);

            var runner = new DynamicCountThreadsRunner(runnerConfig);

            // Act
            runner.Run(action1, action2, action3, action3);

            // Assert
            Thread.Sleep(400);

            Assert.NotNull(thread1);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread1);
            Assert.NotEqual(thread1, thread2);
            Assert.Equal(2, runner.WorkerThreadsCount);
        }

        [Fact]
        public void Run_BelowTreshold_ThreadReleasedCreated()
        {
            // Arrange
            int? thread1 = null;
            Action action1 = () => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(300); };

            int? thread2 = null;
            Action action2 = () => { thread2 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            Action action3 = () => Thread.Sleep(30);

            var runnerConfig = new DynamicCountThreadsRunnerConfig(TimeSpan.FromMilliseconds(50), 2, 2, 5, 2);

            var runner = new DynamicCountThreadsRunner(runnerConfig);

            // Act
            runner.Run(action1, action2, action3, action3);

            // Assert
            Thread.Sleep(400);

            Assert.NotNull(thread1);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread1);
            Assert.NotEqual(thread1, thread2);
            Assert.Equal(2, runner.WorkerThreadsCount);

            Thread.Sleep(100);
            Assert.Equal(1, runner.WorkerThreadsCount);
        }

        [Fact]
        public void Run_OverTreshold_MaxThreadsNotExceeded()
        {
            // Arrange
            Action action1 = () => Thread.Sleep(500);

            Action action2 = () => Thread.Sleep(400);

            var runnerConfig = new DynamicCountThreadsRunnerConfig(TimeSpan.FromMilliseconds(10), 2, 2, 200, 2);

            var runner = new DynamicCountThreadsRunner(runnerConfig);

            // Act
            runner.Run(action1, action2, action2, action2, action2);

            // Assert
            Thread.Sleep(600);

            Assert.Equal(2, runner.WorkerThreadsCount);
        }

        [Fact]
        public void Run_DisposeWhileRunning_ExitsGracefully()
        {
            // Arrange
            var action1 = A.Fake<IAction>();
            A.CallTo(() => action1.Action()).Invokes(() => Thread.Sleep(1000));

            var action2 = A.Fake<IAction>();
            A.CallTo(() => action2.Action()).Invokes(() => Thread.Sleep(10));

            // Act
            using (var runner = new DynamicCountThreadsRunner(DynamicCountThreadsRunnerConfig.Default()))
            {
                runner.Run(action1.Action, action2.Action);
                Thread.Sleep(500);
            }

            // Assert
            Thread.Sleep(1000);

            A.CallTo(() => action1.Action())
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => action2.Action())
             .MustNotHaveHappened();
        }

        [Fact]
        public void Run_DisposeTwiceWhileRunning_ExitsGracefully()
        {
            // Arrange
            var action1 = A.Fake<IAction>();
            A.CallTo(() => action1.Action()).Invokes(() => Thread.Sleep(50));

            var action2 = A.Fake<IAction>();
            A.CallTo(() => action2.Action()).Invokes(() => Thread.Sleep(10));

            // Act
            using (var runner = new DynamicCountThreadsRunner(new DynamicCountThreadsRunnerConfig(TimeSpan.FromMilliseconds(10), 2, 2, 2, 2)))
            {
                runner.Run(action1.Action);
                Thread.Sleep(20);
                runner.Dispose();
            }

            // Assert
            Thread.Sleep(1000);

            A.CallTo(() => action1.Action())
             .MustHaveHappenedOnceExactly();

            A.CallTo(() => action2.Action())
             .MustNotHaveHappened();
        }
    }
}
