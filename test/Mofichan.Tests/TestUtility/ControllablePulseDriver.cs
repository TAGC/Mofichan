using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Tests.TestUtility
{
    public class ControllablePulseDriver : IPulseDriver
    {
        public event EventHandler PulseOccurred;

        public void Pulse()
        {
            this.PulseOccurred?.Invoke(this, EventArgs.Empty);
        }
    }
}
