using System.Collections.Generic;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object used to classify messages by analysing their contents
    /// and context and associating them with an appropriate set of tags.
    /// </summary>
    /// <remarks>
    /// The purpose of tagging messages is that it makes it easier to judge the
    /// semantics of a particular message and therefore let Mofichan choose the
    /// appropriate response for it.
    /// </remarks>
    public interface IMessageClassifier
    {
        /// <summary>
        /// Attempts to classify the provided message. This method will return a set of
        /// tags that represents all classifications that are judged applicable
        /// for the message.
        /// </summary>
        /// <param name="message">The message to attempt to classify.</param>
        /// <returns>The set of tags that are applicable for the message. May be empty.</returns>
        IEnumerable<string> Classify(string message);
    }
}
