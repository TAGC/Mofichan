using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Visitor;
using static Mofichan.Core.Flow.AutoFlowManager;
using Connection = System.Tuple<string, string, string>;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A type of <see cref="IFlowManager"/> that maintains a single flow that is automatically driven.
    /// <para></para>
    /// This type of flow is ideally suited to handling complex "background" behaviours that involve
    /// state changed occurring fully or semi-independently of user interaction.
    /// </summary>
    public class AutoFlowManager : IFlowManager<AutoFlow>
    {
        private readonly AutoFlow template;
        private AutoFlow flow;

        private AutoFlowManager(AutoFlow template)
        {
            this.template = template;
        }

        /// <summary>
        /// Creates a new <c>AutoFlowManager</c> that manages a flow based on the configured template.
        /// </summary>
        /// <param name="buildTemplate">A callback to configure the template used by the manager.</param>
        /// <returns>A new flow manager.</returns>
        public static AutoFlowManager Create(
            Func<BaseFlow.Builder<AutoFlow>, BaseFlow.Builder<AutoFlow>> buildTemplate)
        {
            var builder = new AutoFlow.Builder();
            var template = buildTemplate(builder).Build();

            return new AutoFlowManager(template);
        }

        /// <summary>
        /// Allows this flow manager to generate new flows or modify the state of
        /// existing ones based on the visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public void Accept(IBehaviourVisitor visitor)
        {
            if (this.flow == null)
            {
                this.flow = this.template.Copy();
            }

            this.flow.Accept(visitor);

            if (this.flow.IsComplete)
            {
                this.flow = null;
            }
        }

        /// <summary>
        /// A type of flow that is more-or-less automatically driven.
        /// It is ideal for modelling complex "background" behaviour.
        /// </summary>
        public sealed class AutoFlow : BaseFlow
        {
            private IFlowNode lastActiveNode;

            private AutoFlow(
                string startNodeId,
                IEnumerable<IFlowNode> nodes,
                IEnumerable<IFlowTransition> transitions,
                IList<Connection> connections)
                : base(startNodeId, nodes, transitions, connections)
            {
            }

            /// <summary>
            /// Creates a copy of this flow, which includes copies of all of its nodes, transitions and connections.
            /// </summary>
            /// <returns>A copy of this flow.</returns>
            public new AutoFlow Copy()
            {
                var startNodeId = this.StartNodeId;
                var nodes = this.Nodes.Select(it => it.Copy());
                var transitions = this.Transitions.Select(it => it.Copy());

                return new AutoFlow(startNodeId, nodes, transitions, this.Connections);
            }

            /// <summary>
            /// Allows this flow to respond to a logical clock tick.
            /// </summary>
            /// <param name="visitor">The visitor associated with the tick.</param>
            protected override void Step(OnPulseVisitor visitor)
            {
                var activeNodeDormant = this.ActiveNode.State == FlowNodeState.Dormant;

                if (this.MessageQueue.Any() && !activeNodeDormant)
                {
                    this.FlowContext.Message = this.MessageQueue.Dequeue();
                    this.ActiveNode.Accept(this.FlowContext, visitor);
                }
                else if (this.ActiveNode != this.lastActiveNode && !activeNodeDormant)
                {
                    this.FlowContext.Message = null;
                    this.ActiveNode.Accept(this.FlowContext, visitor);
                    this.lastActiveNode = this.ActiveNode;
                }

                this.ActiveNode.OnTick(this.FlowContext, visitor);
            }

            /// <summary>
            /// Creates a copy of this flow. This is a helper method used with <see cref="Copy" />.
            /// </summary>
            /// <returns>
            /// A copy of this flow.
            /// </returns>
            protected override IFlow GetCopy()
            {
                return this.Copy();
            }

            /// <summary>
            /// Builds instances of <see cref="AutoFlow"/>. 
            /// </summary>
            public class Builder : Builder<AutoFlow>
            {
                /// <summary>
                /// Builds an <c>AutoFlow</c> based on the configuration of this builder.
                /// </summary>
                /// <returns>
                /// An <c>AutoFlow</c>.
                /// </returns>
                public override AutoFlow Build()
                {
                    return new AutoFlow(this.StartNodeId, this.Nodes, this.Transitions, this.Connections);
                }
            }
        }
    }
}
