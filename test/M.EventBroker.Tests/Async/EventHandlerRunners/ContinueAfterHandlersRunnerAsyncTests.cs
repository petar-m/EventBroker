using M.EventBroker.EvenHandlerRunners;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace M.EventBroker.Async.EvenHandlerRunners.Tests
{
    public class ContinueAfterHandlersRunnerAsyncTests
    {
        [Fact]
        public async Task Run_WithMultipleActions_ContinuesAfterHandlers()
        {
            // Arrange
            int? thread1 = null;
            Func<Task> action1 = async () => { thread1 = Thread.CurrentThread.ManagedThreadId; await Task.Delay(100).ConfigureAwait(false); };

            int? thread2 = null;
            Func<Task> action2 = async () => { thread2 = Thread.CurrentThread.ManagedThreadId; await Task.Delay(100).ConfigureAwait(false); };

            var runner = new ContinueAfterHandlersRunnerAsync();

            // Act
            await runner.RunAsync(action1, action2).ConfigureAwait(false);

            // Assert
            Assert.NotNull(thread1);
            Assert.NotNull(thread2);
        }
    }
}

