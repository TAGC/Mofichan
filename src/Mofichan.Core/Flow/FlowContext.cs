using System;
using System.Dynamic;
using Mofichan.Core.Interfaces;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// Represents contextual information and state within a <see cref="IFlow"/>. 
    /// </summary>
    public class FlowContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowContext"/> class.
        /// </summary>
        /// <param name="flowTransitionSelector">The flow transition selector.</param>
        /// <param name="generatedResponseHandler">The callback to handle responses generated within the flow.</param>
        public FlowContext(
            IFlowTransitionSelector flowTransitionSelector,
            IAttentionManager attentionManager,
            Action<OutgoingMessage> generatedResponseHandler)
            : this(default(MessageContext), flowTransitionSelector, attentionManager, generatedResponseHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowContext"/> class.
        /// </summary>
        /// <param name="message">The message context to embed within the flow context.</param>
        /// <param name="flowTransitionSelector">The flow transition selector.</param>
        /// <param name="generatedResponseHandler">The callback to handle responses generated within the flow.</param>
        public FlowContext(
            MessageContext message,
            IFlowTransitionSelector flowTransitionSelector,
            IAttentionManager attentionManager,
            Action<OutgoingMessage> generatedResponseHandler)
        {
            this.Message = message;
            this.FlowTransitionSelector = flowTransitionSelector;
            this.Attention = attentionManager;
            this.GeneratedResponseHandler = generatedResponseHandler;
            this.Extras = new ExpandoObject();
        }

        /// <summary>
        /// Gets the message context.
        /// </summary>
        /// <value>
        /// The message context.
        /// </value>
        public MessageContext Message { get; }

        /// <summary>
        /// Gets the flow transition selector.
        /// </summary>
        /// <value>
        /// The flow transition selector.
        /// </value>
        public IFlowTransitionSelector FlowTransitionSelector { get; }

        /// <summary>
        /// Gets the flow-driven attention manager.
        /// </summary>
        /// <value>
        /// The attention manager.
        /// </value>
        public IAttentionManager Attention { get; }

        /// <summary>
        /// Gets the generated response handler.
        /// </summary>
        /// <value>
        /// The generated response handler.
        /// </value>
        public Action<OutgoingMessage> GeneratedResponseHandler { get; }

        /// <summary>
        /// Gets a(n) <see cref="ExpandoObject"/> that can be used to store additional flow context
        /// information. 
        /// </summary>
        /// <value>
        /// An <c>ExpandObject</c> for storing additional context information.
        /// </value>
        public dynamic Extras { get; private set; }

        /// <summary>
        /// Derives an instance of <c>FlowContext</c> that embeds the specified <c>MessageContext</c>.
        /// </summary>
        /// <param name="message">The message context to embed.</param>
        /// <returns>A new <c>FlowContext</c> instance derived from this.</returns>
        public FlowContext FromMessage(MessageContext message)
        {
            var newContext = new FlowContext(message, this.FlowTransitionSelector, this.Attention,
                this.GeneratedResponseHandler);

            newContext.Extras = this.Extras;

            return newContext;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as FlowContext;

            if (other == null)
            {
                return false;
            }

            bool messagesEqual = this.Message.Equals(other.Message);
            bool transitionSelectorsEqual = this.FlowTransitionSelector.Equals(other.FlowTransitionSelector);
            bool responseHandlersEqual = this.GeneratedResponseHandler.Equals(other.GeneratedResponseHandler);

            return messagesEqual && transitionSelectorsEqual && responseHandlersEqual;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 17;

            hashCode += 31 * this.Message.GetHashCode();
            hashCode += 31 * this.FlowTransitionSelector.GetHashCode();
            hashCode += 31 * this.GeneratedResponseHandler.GetHashCode();

            return hashCode;
        }
    }
}
