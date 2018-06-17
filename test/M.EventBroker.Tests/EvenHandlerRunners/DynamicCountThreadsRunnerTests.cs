using M.EventBroker.EvenHandlerRunners;
using System;
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
    }
}
