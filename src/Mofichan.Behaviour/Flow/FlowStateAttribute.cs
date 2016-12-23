using System;

namespace Mofichan.Behaviour.Flow
{
    /// <summary>
    /// A(n) <see cref="Attribute"/> that allows methods to represent nodes within a(n)
    /// <see cref="Core.Interfaces.IFlow"/>.   
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class FlowStateAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowStateAttribute" /> class.
        /// </summary>
        /// <param name="id">The flow node identifier.</param>
        /// <param name="distinctUntilChanged">
        /// If set to <c>true</c>, will cause the node to ignore <see cref="Core.Flow.FlowContext"/> instances
        /// until they change.
        /// </param>
        public FlowStateAttribute(string id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the flow node identifier.
        /// </summary>
        /// <value>
        /// The flow node identifier.
        /// </value>
        public string Id { get; }
    }
}
