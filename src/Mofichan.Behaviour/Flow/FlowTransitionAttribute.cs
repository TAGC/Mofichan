using System;

namespace Mofichan.Behaviour.Flow
{
    /// <summary>
    /// A(n) <see cref="Attribute"/> that allows methods to represent transitions within a(n)
    /// <see cref="Core.Interfaces.IFlow"/>.   
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class FlowTransitionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowTransitionAttribute"/> class.
        /// </summary>
        /// <param name="id">The transition identifier.</param>
        /// <param name="from">The identifier of the node to transition from.</param>
        /// <param name="to">The identifier of the node to transition to.</param>
        public FlowTransitionAttribute(string id, string from, string to)
        {
            this.Id = id;
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Gets the transition identifier.
        /// </summary>
        /// <value>
        /// The transition identifier.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets the identifier of the node to transition from.
        /// </summary>
        /// <value>
        /// The identifier of the node to transition from.
        /// </value>
        public string From { get; }

        /// <summary>
        /// Gets the identifier of the node to transition to.
        /// </summary>
        /// <value>
        /// The identifier of the node to transition to.
        /// </value>
        public string To { get; }
    }
}
