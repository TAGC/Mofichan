using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;
using static Mofichan.Core.Flow.UserDrivenFlowManager;
using Connection = System.Tuple<string, string, string>;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A type of <see cref="IFlowManager"/> that maintains a pool of flows, each of which is
    /// associated with a particular user.
    /// <para></para>
    /// These types of flows are ideally suited to handling "conversational" flows between Mofichan
    /// and a particular user. Flows are driven by interaction with the user.
    /// </summary>
    public class UserDrivenFlowManager : IFlowManager<UserDrivenFlow>
    {
        private readonly UserDrivenFlow template;
        private readonly IDictionary<string, UserDrivenFlow> flows;

        private UserDrivenFlowManager(UserDrivenFlow template)
        {
            Raise.ArgumentNullException.IfIsNull(template, nameof(template));

            this.template = template;
            this.flows = new Dictionary<string, UserDrivenFlow>();
        }

        /// <summary>
        /// Creates a new <c>UserDrivenFlowManager</c> that manages a flow based on the configured template.
        /// </summary>
        /// <param name="buildTemplate">A callback to configure the template used by the manager.</param>
        /// <returns>A new flow manager.</returns>
        public static UserDrivenFlowManager Create(
            Func<BaseFlow.Builder<UserDrivenFlow>, BaseFlow.Builder<UserDrivenFlow>> buildTemplate)
        {
            var builder = new UserDrivenFlow.Builder();
            var template = buildTemplate(builder).Build();

            return new UserDrivenFlowManager(template);
        }

        /// <summary>
        /// Allows this flow manager to generate new flows or modify the state of
        /// existing ones based on the visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public void Accept(IBehaviourVisitor visitor)
        {
            var onMessageVisitor = visitor as OnMessageVisitor;
            var onPulseVisitor = visitor as OnPulseVisitor;

            if (onMessageVisitor != null)
            {
                var sender = onMessageVisitor.Message.From as IUser;
                Debug.Assert(sender != null, "The message should be from a user");

                UserDrivenFlow flow;

                if (!this.flows.TryGetValue(sender.UserId, out flow))
                {
                    flow = this.template.Copy();
                    this.flows[sender.UserId] = flow;
                }

                flow.Accept(onMessageVisitor);
            }
            else if (onPulseVisitor != null)
            {
                this.flows.Values.ToList().ForEach(it => it.Accept(onPulseVisitor));
            }

            foreach (var item in this.flows.ToList())
            {
                if (item.Value.IsComplete)
                {
                    this.flows.Remove(item);
                }
            }
        }

        /// <summary>
        /// A type of flow that is mainly driven through interaction with a user.
        /// It is ideal for modelling conversational flows.
        /// </summary>
        public sealed class UserDrivenFlow : BaseFlow
        {
            private UserDrivenFlow(
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
            public new UserDrivenFlow Copy()
            {
                var startNodeId = this.StartNodeId;
                var nodes = this.Nodes.Select(it => it.Copy());
                var transitions = this.Transitions.Select(it => it.Copy());

                return new UserDrivenFlow(startNodeId, nodes, transitions, this.Connections);
            }

            /// <summary>
            /// Allows this flow to respond to a logical clock tick.
            /// </summary>
            /// <param name="visitor">The visitor associated with the tick.</param>
            protected override void Step(OnPulseVisitor visitor)
            {
                if (this.MessageQueue.Any() && this.ActiveNode.State != FlowNodeState.Dormant)
                {
                    IFlowNode lastActiveNode;
                    var message = this.MessageQueue.Dequeue();

                    do
                    {
                        lastActiveNode = this.ActiveNode;
                        this.Process(message, visitor);
                        lastActiveNode.OnTick(this.FlowContext, visitor);
                    }
                    while (!this.IsComplete && this.ActiveNode != lastActiveNode);
                }
                else
                {
                    this.ActiveNode.OnTick(this.FlowContext, visitor);
                }
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

            private void Process(MessageContext message, OnPulseVisitor visitor)
            {
                this.FlowContext.Message = message;
                this.ActiveNode.Accept(this.FlowContext, visitor);
            }

            /// <summary>
            /// Builds instances of <see cref="UserDrivenFlow"/>. 
            /// </summary>
            public class Builder : Builder<UserDrivenFlow>
            {
                /// <summary>
                /// Builds a <c>UserDrivenFlow</c> based on the configuration of this builder.
                /// </summary>
                /// <returns>
                /// A <c>UserDrivenFlow</c>.
                /// </returns>
                public override UserDrivenFlow Build()
                {
                    return new UserDrivenFlow(this.StartNodeId, this.Nodes, this.Transitions, this.Connections);
                }
            }
        }
    }
}
