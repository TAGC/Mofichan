using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents a pluggable module that can affect Mofichan's behaviour.
    /// <para></para>
    /// Instances of this class will typically control how Mofichan responds to
    /// messages, but may also allow her to have "passive" behaviours that
    /// cause her to act independently of received messages.
    /// </summary>
    public interface IMofichanBehaviour : ISubject<IncomingMessage>, ISubject<OutgoingMessage>
    {
        /// <summary>
        /// Gets the behaviour module identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; }

        /// <summary>
        /// Allows the behaviour to inspect the stack of behaviours Mofichan
        /// will be loaded with.
        /// </summary>
        /// <remarks>
        /// This method should be invoked before the behaviour <i>chain</i>
        /// is created.
        /// </remarks>
        /// <param name="stack">The behaviour stack.</param>
        void InspectBehaviourStack(IList<IMofichanBehaviour> stack);

        /// <summary>
        /// Initialises the behaviour module.
        /// </summary>
        void Start();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        string ToString();
    }
}
