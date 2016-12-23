using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core
{
    /// <summary>
    /// Represents a message and associated contextual details.
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageContext"/> class.
        /// </summary>
        public MessageContext() : this(null, null, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageContext" /> class.
        /// </summary>
        /// <param name="from">The sender of the message.</param>
        /// <param name="to">The recipient of the message.</param>
        /// <param name="body">The message body.</param>
        /// <param name="delay">An optional delay that should be waited before sending the message.</param>
        /// <param name="created">The creation time of this message context (defaults to "now").</param>
        /// <param name="tags">An optional set of tags associated with this message, if classified.</param>
        public MessageContext(IMessageTarget from, IMessageTarget to, string body, TimeSpan? delay = null,
            DateTime? created = null, IEnumerable<string> tags = null)
        {
            this.From = from;
            this.To = to;
            this.Body = body;
            this.Delay = delay ?? TimeSpan.Zero;
            this.Tags = tags ?? Enumerable.Empty<string>();
            this.Created = created ?? DateTime.Now;
        }

        /// <summary>
        /// Gets the message sender.
        /// </summary>
        /// <value>
        /// The message sender.
        /// </value>
        public IMessageTarget From { get; }

        /// <summary>
        /// Gets the message recipient.
        /// </summary>
        /// <value>
        /// The message recipient.
        /// </value>
        public IMessageTarget To { get; }

        /// <summary>
        /// Gets the message body.
        /// </summary>
        /// <value>
        /// The message body.
        /// </value>
        public string Body { get; }

        /// <summary>
        /// Gets the amount of time that should be waited before sending this message (if applicable).
        /// </summary>
        /// <value>
        /// The message send delay.
        /// </value>
        public TimeSpan Delay { get; }

        /// <summary>
        /// Gets the time that this message context was created.
        /// </summary>
        /// <value>
        /// The message context creation time.
        /// </value>
        public DateTime Created { get; }

        /// <summary>
        /// Gets the tags associated with the message during classification.
        /// </summary>
        /// <value>
        /// The message tags.
        /// </value>
        public IEnumerable<string> Tags { get; }

        /// <summary>
        /// Derives a <c>MessageContext</c> from this instance with <paramref name="tags"/>
        /// concatenated to this instance's collection of tags.
        /// </summary>
        /// <param name="tags">The tags to concatenate.</param>
        /// <returns>A derived instance of <c>MessageContext</c>.</returns>
        public MessageContext FromTags(IEnumerable<string> tags)
        {
            Raise.ArgumentNullException.IfIsNull(tags, nameof(tags));
            return new MessageContext(this.From, this.To, this.Body, this.Delay, this.Created, this.Tags.Concat(tags));
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
            var other = obj as MessageContext;

            if (other == null)
            {
                return false;
            }

            var sendersEqual = (this.From == null && other.From == null) || this.From.Equals(other.From);
            var recipientsEqual = (this.To == null && other.To == null) || this.To.Equals(other.To);
            var bodiesEqual = (this.Body == null && other.Body == null) || this.Body.Equals(other.Body);
            var delaysEqual = this.Delay.Equals(other.Delay);
            var createdEqual = this.Created.Equals(other.Created);
            var tagsEqual = this.Tags.SequenceEqual(other.Tags);

            return sendersEqual && recipientsEqual && bodiesEqual && delaysEqual && createdEqual && tagsEqual;
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

            hashCode += 31 * this.From?.GetHashCode() ?? 0;
            hashCode += 31 * this.To?.GetHashCode() ?? 0;
            hashCode += 31 * this.Body?.GetHashCode() ?? 0;
            hashCode += 31 * this.Delay.GetHashCode();
            hashCode += 31 * this.Created.GetHashCode();

            foreach (var tag in this.Tags)
            {
                hashCode += 31 * tag.GetHashCode();
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
            return string.Format("Message '{2}' ({0} -> {1}, delay={3}, created={4}, tags={5})",
                this.From, this.To, this.Body, this.Delay, this.Created, string.Join(",", this.Tags));
        }
    }
}
