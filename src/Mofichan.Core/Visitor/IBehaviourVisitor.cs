using System;
using System.Collections.Generic;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// Represents a type of object that "visits" behaviours and allows them to register
    /// responses.
    /// </summary>
    public interface IBehaviourVisitor
    {
        /// <summary>
        /// Gets the collection of responses that have been registered to this visitor so far.
        /// </summary>
        /// <value>
        /// The registered responses.
        /// </value>
        IEnumerable<Response> Responses { get; }

        /// <summary>
        /// Registers a response.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the response builder.</param>
        void RegisterResponse(Action<Response.Builder> configureBuilder);

        /// <summary>
        /// Modifies all currently registered responses.
        /// </summary>
        /// <param name="modification">The modification to apply to each response.</param>
        void ModifyResponses(Func<Response, Response> modification);
    }
}
