using System;
using Mofichan.Core.Visitor;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object that's used to choose a response for a particular
    /// message received from a user.
    /// </summary>
    public interface IResponseSelector
    {
        /// <summary>
        /// Occurs when this object has chosen a response to a particular message.
        /// </summary>
        event EventHandler<ResponseSelectedEventArgs> ResponseSelected;

        /// <summary>
        /// Occurs when the window for responding to a particular message has expired.
        /// </summary>
        event EventHandler<ResponseWindowExpiredEventArgs> ResponseWindowExpired;

        /// <summary>
        /// Inspects the specified visitor to determine which messages have been received
        /// and what response candidates are available for them.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void InspectVisitor(IBehaviourVisitor visitor);
    }

    /// <summary>
    /// Represents the arguments during a response selection event.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ResponseSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseSelectedEventArgs"/> class.
        /// </summary>
        /// <param name="response">The selected response.</param>
        public ResponseSelectedEventArgs(Response response)
        {
            this.Response = response;
        }

        /// <summary>
        /// Gets the selected response.
        /// </summary>
        /// <value>
        /// The selected response.
        /// </value>
        public Response Response { get; }

        /// <summary>
        /// Gets the message being responded to.
        /// </summary>
        /// <value>
        /// The message being responding to.
        /// </value>
        public MessageContext RespondingTo
        {
            get
            {
                return this.Response.RespondingTo;
            }
        }
    }

    /// <summary>
    /// Represents the arguments during a response window expiration event.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ResponseWindowExpiredEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseWindowExpiredEventArgs"/> class.
        /// </summary>
        /// <param name="respondingTo">The message that the response window expired for.</param>
        public ResponseWindowExpiredEventArgs(MessageContext respondingTo)
        {
            this.RespondingTo = respondingTo;
        }

        /// <summary>
        /// Gets the message that the response window expired for.
        /// </summary>
        /// <value>
        /// The message that the response window expired for.
        /// </value>
        public MessageContext RespondingTo { get; }
    }
}
