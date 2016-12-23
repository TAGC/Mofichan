using System;
using System.Collections.Generic;
using Mofichan.Core.BehaviourOutputs;

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
        /// Gets the collections of outputs that have been autonomously generated and registered to
        /// this visitor so far.
        /// </summary>
        /// <value>
        /// The autonomous behaviour outputs.
        /// </value>
        IEnumerable<SimpleOutput> AutonomousOutputs { get; }

        /// <summary>
        /// Registers a response.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the response builder.</param>
        void RegisterResponse(Action<Response.Builder> configureBuilder);

        /// <summary>
        /// Registers an autonomous behaviour output.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the autonomous output builder.</param>
        void RegisterAutonomousOutput(Action<SimpleOutput.Builder> configureBuilder);

        /// <summary>
        /// Modifies all currently registered responses.
        /// </summary>
        /// <param name="modification">The modification to apply to each response.</param>
        void ModifyResponses(Func<Response, Response> modification);

        /// <summary>
        /// Modifies all currently registered autonomous outputs.
        /// </summary>
        /// <param name="modification">The modification to apply to each output.</param>
        void ModifyAutonomousOutputs(Func<SimpleOutput, SimpleOutput> modification);
    }
}
