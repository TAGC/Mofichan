using System.Collections.Generic;
using System.Linq;

namespace Mofichan.Core.Relevance
{
    /// <summary>
    /// Represents an argument about the relevance of particular objects based on contextual information.
    /// </summary>
    public class RelevanceArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelevanceArgument"/> class.
        /// </summary>
        /// <param name="messageTagArguments">
        /// A collection of message classifications. When matched against a message, the
        /// argument will be considered stronger if the message possesses more of these classifications.
        /// </param>
        /// <param name="guaranteeRelevance">if set to <c>true</c> [guarantee relevance].</param>
        public RelevanceArgument(IEnumerable<string> messageTagArguments, bool guaranteeRelevance)
        {
            this.MessageTagArguments = messageTagArguments;
            this.GuaranteeRelevance = guaranteeRelevance;
        }

        /// <summary>
        /// Gets a value indicating whether this argument includes a guarantee of relevance based
        /// on the context.
        /// <para></para>
        /// At most only one argument should guarantee relevance for a particular situation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if relevance is guaranteed; otherwise, <c>false</c>.
        /// </value>
        public bool GuaranteeRelevance { get; }

        /// <summary>
        /// Gets the message tag arguments. When this argument is matched against a message, its strength
        /// should in part be determined by how many of these classifications the message has.
        /// </summary>
        /// <value>
        /// The message tag arguments.
        /// </value>
        public IEnumerable<string> MessageTagArguments { get; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as RelevanceArgument;

            if (other == null)
            {
                return false;
            }

            return this.GuaranteeRelevance == other.GuaranteeRelevance
                && this.MessageTagArguments.SequenceEqual(other.MessageTagArguments);
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

            hashCode += 31 * this.GuaranteeRelevance.GetHashCode();

            foreach (var messageTagArgument in this.MessageTagArguments)
            {
                hashCode += 31 * messageTagArgument.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.GuaranteeRelevance)
            {
                return "Relevance argument - relevance guaranteed";
            }
            else
            {
                var tags = string.Join(", ", this.MessageTagArguments.Select(it => "#" + it));
                return string.Format("Relevance argument - suits message tags: {0}", tags);
            }
        }

        /// <summary>
        /// Used to construct instances of <see cref="RelevanceArgument"/>. 
        /// </summary>
        public class Builder
        {
            private readonly ISet<string> messageTagArguments;
            private bool guaranteeRelevance;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            public Builder()
            {
                this.messageTagArguments = new HashSet<string>();
            }

            /// <summary>
            /// Specifies that the argument is relevant for messages associated with the provided
            /// set of classifications.
            /// </summary>
            /// <param name="tags">The message classifications.</param>
            /// <returns>This builder.</returns>
            public Builder SuitsMessageTags(params string[] tags)
            {
                this.messageTagArguments.UnionWith(tags);
                return this;
            }

            /// <summary>
            /// Specifies that the argument should guarantee relevance for the particular situation.
            /// </summary>
            /// <returns>This builde.r</returns>
            public Builder GuaranteesRelevance()
            {
                this.guaranteeRelevance = true;
                return this;
            }

            /// <summary>
            /// Builds a <see cref="RelevanceArgument"/> based on the configuration of this builder.
            /// </summary>
            /// <returns>A new relevance argument.</returns>
            public RelevanceArgument Build()
            {
                return new RelevanceArgument(this.messageTagArguments, this.guaranteeRelevance);
            }
        }
    }
}
