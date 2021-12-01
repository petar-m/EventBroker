using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace M.EventBroker.Async.EvenHandlerRunners.Tests
{
    public class UnrestrictedThreadPoolRunnerAsyncTests
    {
        [Fact]
        public async Task Run_WithMultipleActions_ActionsAreRunnedOnDifferentThreads()
        {
            // Arrange
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;

            int? thread1 = null;
            Func<Task> action1 = async () => { thread1 = Thread.CurrentThread.ManagedThreadId; await Task.Delay(30).ConfigureAwait(false); };

            int? thread2 = null;
            Func<Task> action2 = async () => { thread2 = Thread.CurrentThread.ManagedThreadId; await Task.Delay(30).ConfigureAwait(false); };

            var runner = new UnrestrictedThreadPoolRunnerAsync();

            // Act
            await runner.RunAsync(action1, action2).ConfigureAwait(false);

            // Assert
            Thread.Sleep(100);

            Assert.NotNull(thread1);
            Assert.NotEqual(currentThreadId, thread1);

            Assert.NotNull(thread2);
            Assert.NotEqual(currentThreadId, thread2);

            Assert.NotEqual(thread1, thread2);
        }
    }
}

