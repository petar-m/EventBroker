using M.EventBroker.EvenHandlerRunners;
using System;
using Xunit;

namespace M.EventBroker.Tests.EvenHandlerRunners
{
    public class DynamicCountThreadsRunnerConfigTests
    {
        [Fact]
        public void Constructor_WithValidArguments_InitializesInstance()
        {
            var config = new DynamicCountThreadsRunnerConfig(TimeSpan.FromSeconds(5), 1, 2, 3, 4);

            Assert.Equal(TimeSpan.FromSeconds(5), config.CheckInterval);
            Assert.Equal(1, config.WaitingHandlersCountTreshold);
            Assert.Equal(2, config.CyclesBeforeAddingThread);
            Assert.Equal(3, config.CyclesBeforeReleasingThread);
            Assert.Equal(4, config.MaxThreadsCount);
        }

        [Fact]
        public void Default_InitializesInstance()
        {
            var config = DynamicCountThreadsRunnerConfig.Default();

            Assert.Equal(TimeSpan.FromMilliseconds(100), config.CheckInterval);
            Assert.Equal(2, config.WaitingHandlersCountTreshold);
            Assert.Equal(2, config.CyclesBeforeAddingThread);
            Assert.Equal(20, config.CyclesBeforeReleasingThread);
            Assert.Equal(5, config.MaxThreadsCount);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Constructor_WithInvalidCheckInterval_ThrowsException(int seconds)
        {
            Action ctor = () => new DynamicCountThreadsRunnerConfig(TimeSpan.FromSeconds(seconds), 1, 1, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(ctor);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Constructor_WithInvalidWaitingHandlersCountTreshold_ThrowsException(int waitingHandlersCountTreshold)
        {
            Action ctor = () => new DynamicCountThreadsRunnerConfig(TimeSpan.FromSeconds(1), waitingHandlersCountTreshold, 1, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(ctor);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Constructor_WithInvalidCyclesBeforeAddingThread_ThrowsException(int cyclesBeforeAddingThread)
        {
            Action ctor = () => new DynamicCountThreadsRunnerConfig(TimeSpan.FromSeconds(1), 1, cyclesBeforeAddingThread, 1, 1);
            Assert.Throws<ArgumentOutOfRangeException>(ctor);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Constructor_WithInvalidCyclesBeforeReleasingThread_ThrowsException(int cyclesBeforeReleasingThread)
        {
            Action ctor = () => new DynamicCountThreadsRunnerConfig(TimeSpan.FromSeconds(1), 1, 1, cyclesBeforeReleasingThread, 1);
            Assert.Throws<ArgumentOutOfRangeException>(ctor);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Constructor_WithInvalidMaxThreadsCount_ThrowsException(int maxThreadsCount)
        {
            Action ctor = () => new DynamicCountThreadsRunnerConfig(TimeSpan.FromSeconds(1), 1, 1, 1, maxThreadsCount);
            Assert.Throws<ArgumentOutOfRangeException>(ctor);
        }
    }
}
