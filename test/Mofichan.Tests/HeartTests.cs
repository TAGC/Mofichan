using System;
using System.Threading;
using Mofichan.Core;
using Mofichan.Tests.TestUtility;
using Xunit;

namespace Mofichan.Tests
{
    public class HeartTests
    {
        [Fact]
        public void Heart_Should_Pulse_Periodically()
        {
            // WHEN we create a heart.
            var resetEvent = new AutoResetEvent(false);
            using (var heart = new Heart(MockLogger.Instance))
            {
                heart.PulseOccurred += (s, e) => resetEvent.Set();
                
                // AND we set the pulse rate to 10 milliseconds.
                heart.Rate = TimeSpan.FromMilliseconds(10);

                // THEN it should begin firing periodically.
                for (int i = 0; i < 10; i++)
                {
                    resetEvent.WaitOne(100);
                    resetEvent.Reset();
                }
            }
        }
    }
}
