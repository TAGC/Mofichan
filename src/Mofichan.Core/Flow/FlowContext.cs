using System.Dynamic;

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
        public FlowContext()
        {
            this.Extras = new ExpandoObject();
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public MessageContext Message { get; set; }

        /// <summary>
        /// Gets a(n) <see cref="ExpandoObject"/> that can be used to store additional flow context
        /// information. 
        /// </summary>
        /// <value>
        /// An <c>ExpandObject</c> for storing additional context information.
        /// </value>
        public dynamic Extras { get; private set; }

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

            bool messagesEqual = (this.Message == null && other.Message == null)
                || this.Message.Equals(other.Message);

            return messagesEqual;
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

            return hashCode;
        }
    }
}
