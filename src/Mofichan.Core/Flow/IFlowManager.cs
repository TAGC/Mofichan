using Mofichan.Core.Visitor;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A flow manager is used to create and handle flows in response to received
    /// instances of <see cref="IBehaviourVisitor"/>.
    /// <para></para>
    /// Different flow managers will employ different types of flows.
    /// </summary>
    public interface IFlowManager
    {
        /// <summary>
        /// Allows this flow manager to generate new flows or modify the state of
        /// existing ones based on the visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void Accept(IBehaviourVisitor visitor);
    }

    /// <summary>
    /// See <see cref="IFlowManager"/>. 
    /// </summary>
    /// <typeparam name="T">The type of flow employed by this manager.</typeparam>
    public interface IFlowManager<T> : IFlowManager where T : BaseFlow
    {
    }
}
