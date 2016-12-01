using System;
using Mofichan.Core.Interfaces;

namespace Mofichan.Tests.TestUtility
{
    public class ControllableFlowDriver : IFlowDriver
    {
        public void StepFlow()
        {
            this.OnNextStep?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler OnNextStep;
    }
}
