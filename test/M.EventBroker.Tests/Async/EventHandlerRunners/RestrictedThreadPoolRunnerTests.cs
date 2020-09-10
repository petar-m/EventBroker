using FakeItEasy;
using M.EventBroker.Async.Tests;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace M.EventBroker.Async.EvenHandlerRunners.Tests
{
    public class RestrictedThreadPoolRunnerAsyncTests
    {
        [Fact]
        public async Task Run_WithRestrictionOfTwoAndTwoActions_ActionsAreRunnedOnDifferentThreads()
        {
            // Arrange
            int? thread1 = null;
            Func<Task> action1 = async () => { thread1 = Thread.CurrentThread.ManagedThreadId; await Task.Delay(30).ConfigureAwait(false); };

            int? thread2 = null;
            Func<Task> action2 = async () => { thread2 = Thread.CurrentThread.ManagedThreadId; await Task.Delay(30).ConfigureAwait(false); };

            var runner = new RestrictedThreadPoolRunnerAsync(2);

            // Act
            await runner.RunAsync(action1, action2).ConfigureAwait(false); ;

            // Assert
            Thread.Sleep(100);

            Assert.NotNull(thread1);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread1);

            Assert.NotNull(thread2);
            Assert.NotEqual(Thread.CurrentThread.ManagedThreadId, thread2);

            Assert.NotEqual(thread1, thread2);
        }

        [Fact]
        public async Task Run_WithRestrictionOfOneAndMultipleAcrions_AllActionsAreRunnedAsync()
        {
            // Arrange
            var action1 = A.Fake<IActionAsync>();
            A.CallTo(() => action1.Action()).Invokes(async () => await Task.Delay(50).ConfigureAwait(false));

            var action2 = A.Fake<IActionAsync>();
            A.CallTo(() => action2.Action()).Invokes(async () => await Task.Delay(50).ConfigureAwait(false));

            var action3 = A.Fake<IActionAsync>();
            A.CallTo(() => action3.Action()).Invokes(async () => await Task.Delay(50).ConfigureAwait(false));

            var runner = new RestrictedThreadPoolRunnerAsync(1);

            // Act
            await runner.RunAsync(action1.Action, action2.Action, action3.Action).ConfigureAwait(false);

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
            Action constructor = () => new RestrictedThreadPoolRunnerAsync(-3);
            Assert.Throws<ArgumentOutOfRangeException>(constructor);
        }

        [Fact]
        public void Constructor_WithZeroMaxConcurrentHandlers_ThrowsException()
        {
            Action constructor = () => new RestrictedThreadPoolRunnerAsync(0);
            Assert.Throws<ArgumentOutOfRangeException>(constructor);
        }

        [Fact]
        public async Task Run_DisposeWhileRunning_ExitsGracefullyAsync()
        {
            // Arrange
            var action1 = A.Fake<IActionAsync>();
            A.CallTo(() => action1.Action()).Invokes(async () => await Task.Delay(300).ConfigureAwait(false));

            var action2 = A.Fake<IActionAsync>();
            A.CallTo(() => action2.Action()).Invokes(async () => await Task.Delay(200).ConfigureAwait(false));

            // Act
            using (var runner = new RestrictedThreadPoolRunnerAsync(1))
            {
                await runner.RunAsync(action1.Action, action2.Action).ConfigureAwait(false);
                await Task.Delay(10).ConfigureAwait(false);
                runner.Dispose();
            }

            // Assert
            Thread.Sleep(1000);

            A.CallTo(() => action1.Action())
             .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => action2.Action())
             .MustNotHaveHappened();
        }

        [Fact]
        public async Task Run_DisposeTwiceWhileRunning_ExitsGracefullyAsync()
        {
            // Arrange
            var action1 = A.Fake<IActionAsync>();
            A.CallTo(() => action1.Action()).Invokes(async () => await Task.Delay(1000).ConfigureAwait(false));

            // Act
            using (var runner = new RestrictedThreadPoolRunnerAsync(1))
            {
                await runner.RunAsync(action1.Action).ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);
                runner.Dispose();
            }

            // Assert
            await Task.Delay(100).ConfigureAwait(false);

            A.CallTo(() => action1.Action())
             .MustHaveHappenedOnceExactly();
        }
    }
}
