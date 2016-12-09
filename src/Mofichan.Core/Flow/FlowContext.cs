using System.Dynamic;
using Mofichan.Core.Visitor;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// Represents contextual information and state within a <see cref="IFlow"/>. 
    /// </summary>
    public class FlowContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowContext" /> class.
        /// </summary>
        public FlowContext() : this(default(MessageContext), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowContext" /> class.
        /// </summary>
        /// <param name="message">The message to associate with the flow context.</param>
        /// <param name="visitor">The visitor to associate with the flow context.</param>
        public FlowContext(MessageContext message, IBehaviourVisitor visitor)
        {
            this.Message = message;
            this.Visitor = visitor;
            this.Extras = new ExpandoObject();
        }

        /// <summary>
        /// Gets the visitor.
        /// </summary>
        /// <value>
        /// The visitor.
        /// </value>
        public IBehaviourVisitor Visitor { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public MessageContext Message { get; }

        /// <summary>
        /// Gets a(n) <see cref="ExpandoObject"/> that can be used to store additional flow context
        /// information. 
        /// </summary>
        /// <value>
        /// An <c>ExpandObject</c> for storing additional context information.
        /// </value>
        public dynamic Extras { get; private set; }

        /// <summary>
        /// Derives an instance of <c>FlowContext</c> that includes the specified message.
        /// </summary>
        /// <param name="message">The message to associate with this context.</param>
        /// <returns>A new <c>FlowContext</c> instance derived from this.</returns>
        public FlowContext FromMessage(MessageContext message)
        {
            var newContext = new FlowContext(message, this.Visitor);

            newContext.Extras = this.Extras;

            return newContext;
        }

        /// <summary>
        /// Derives an instance of <c>FlowContext</c> that includes the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor to associate with this context.</param>
        /// <returns>A new <c>FlowContext</c> instance derived from this.</returns>
        public FlowContext FromVisitor(IBehaviourVisitor visitor)
        {
            var newContext = new FlowContext(this.Message, visitor);

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

            bool messagesEqual = (this.Message == null && other.Message == null) || this.Message.Equals(other.Message);
            bool visitorsEqual = (this.Visitor == null && other.Visitor == null) || this.Visitor.Equals(other.Visitor);

            return messagesEqual && visitorsEqual;
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

            hashCode += 31 * this.Message?.GetHashCode() ?? 0;
            hashCode += 31 * this.Visitor?.GetHashCode() ?? 0;

            return hashCode;
        }
    }
}
