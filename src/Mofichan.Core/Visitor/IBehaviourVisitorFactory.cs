namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// Represents a factory used for creating instances of <see cref="IBehaviourVisitor"/>. 
    /// </summary>
    public interface IBehaviourVisitorFactory
    {
        /// <summary>
        /// Creates an <see cref="OnMessageVisitor"/> that carries the message that responses should be
        /// collected for.
        /// </summary>
        /// <param name="message">The received message, which responses should be registered for.</param>
        /// <returns>A new <c>OnMessageVisitor</c>.</returns>
        OnMessageVisitor CreateMessageVisitor(MessageContext message);

        /// <summary>
        /// Creates an <see cref="OnPulseVisitor"/>.
        /// </summary>
        /// <returns>A new <c>OnPulseVisitor</c>.</returns>
        OnPulseVisitor CreatePulseVisitor();
    }
}
