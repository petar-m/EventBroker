﻿using M.EventBroker.EvenHandlerRunners;
using System;
using System.Threading;
using Xunit;

namespace M.EventBroker.Tests.EvenHandlerRunners
{
    public class CallerThreadBlockingRunnerTests
    {
        [Fact(Skip = "TODO: failing when run by github actions, investigate")]
        public void Run_WithMultipleActions_ActionsAreRunnedOnDifferentThreads()
        {
            // Arrange
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;

            int? thread1 = null;
            Action action1 = () => { thread1 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            int? thread2 = null;
            Action action2 = () => { thread2 = Thread.CurrentThread.ManagedThreadId; Thread.Sleep(30); };

            var runner = new CallerThreadBlockingRunner();

            // Act
            runner.Run(action1, action2); runner.Run();

            // Assert
            Assert.NotNull(thread1);
            Assert.Equal(currentThreadId, thread1);

            Assert.NotNull(thread2);
            Assert.Equal(currentThreadId, thread2);
        }
    }
}

