namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an entity that can receive messages (as raw strings).
    /// </summary>
    public interface IMessageTarget
    {
        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="message">The message to process.</param>
        void ReceiveMessage(string message);
    }
}
