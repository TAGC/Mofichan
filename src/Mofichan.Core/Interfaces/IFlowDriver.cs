using System;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object used to drive a behavioural flow by firing events
    /// to signal when the next steps in flows should occur.
    /// </summary>
    public interface IFlowDriver
    {
        /// <summary>
        /// Occurs when flows should perform their next step.
        /// </summary>
        event EventHandler OnNextStep;
    }
}