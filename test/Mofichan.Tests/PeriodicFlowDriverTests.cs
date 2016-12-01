using System;
using System.Threading;
using Mofichan.Core.Flow;
using Moq;
using Serilog;
using Xunit;

namespace Mofichan.Tests
{
    public class PeriodicFlowDriverTests
    {
        [Fact]
        public void Periodic_Flow_Driver_Should_Throw_Exception_If_Provided_With_No_Delay()
        {
            Assert.Throws<ArgumentException>(() => new PeriodicFlowDriver(TimeSpan.Zero, Mock.Of<ILogger>()));
        }

        [Fact]
        public void Periodic_Flow_Driver_Should_Fire_Periodically()
        {
            // WHEN we create a flow driver.
            var resetEvent = new AutoResetEvent(false);
            using (var flowDriver = new PeriodicFlowDriver(TimeSpan.FromMilliseconds(10), Mock.Of<ILogger>()))
            {
                flowDriver.OnNextStep += (s, e) => resetEvent.Set();

                // THEN it should begin firing periodically.
                for (int i = 0; i < 10; i++)
                {
                    resetEvent.WaitOne(100);
                    resetEvent.Reset();
                }
            }
        }

        [Fact]
        public void Periodic_Flow_Driver_Should_Stop_Firing_After_Disposal()
        {
            // GIVEN a running flow driver.
            var resetEvent = new AutoResetEvent(false);
            var flowDriver = new PeriodicFlowDriver(TimeSpan.FromMilliseconds(10), Mock.Of<ILogger>());
            flowDriver.OnNextStep += (s, e) => resetEvent.Set();

            // WHEN we dispose the flow driver.
            flowDriver.Dispose();

            // THEN it should have stopped firing.
            resetEvent.WaitOne(100);
        }
    }
}
