using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core.Visitor
{
    /// <summary>
    /// A base implementation of <see cref="IBehaviourVisitor"/>. 
    /// </summary>
    public abstract class BaseBehaviourVisitor : IBehaviourVisitor
    {
        private readonly List<Response> responses;
        private readonly List<SimpleOutput> autonomousOutputs;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseBehaviourVisitor" /> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="messageBuilderFactory">A factory for constructing instances of a message builder.</param>
        protected BaseBehaviourVisitor(BotContext botContext,
            Func<IResponseBodyBuilder> messageBuilderFactory)
        {
            Raise.ArgumentNullException.IfIsNull(botContext, nameof(botContext));
            Raise.ArgumentNullException.IfIsNull(messageBuilderFactory, nameof(messageBuilderFactory));

            this.BotContext = botContext;
            this.MessageBuilderFactory = messageBuilderFactory;
            this.responses = new List<Response>();
            this.autonomousOutputs = new List<SimpleOutput>();
        }

        /// <summary>
        /// Gets the collection of responses that have been registered to this visitor so far.
        /// </summary>
        /// <value>
        /// The registered responses.
        /// </value>
        public IEnumerable<Response> Responses
        {
            get
            {
                return this.responses;
            }
        }

        /// <summary>
        /// Gets the collections of outputs that have been autonomously generated and registered to
        /// this visitor so far.
        /// </summary>
        /// <value>
        /// The autonomous behaviour outputs.
        /// </value>
        public IEnumerable<SimpleOutput> AutonomousOutputs
        {
            get
            {
                return this.autonomousOutputs;
            }
        }

        /// <summary>
        /// Gets the message builder factory.
        /// </summary>
        /// <value>
        /// The message builder factory.
        /// </value>
        protected Func<IResponseBodyBuilder> MessageBuilderFactory { get; }

        /// <summary>
        /// Gets the bot context.
        /// </summary>
        /// <value>
        /// The bot context.
        /// </value>
        protected BotContext BotContext { get; }

        /// <summary>
        /// Modifies all currently registered responses.
        /// </summary>
        /// <param name="modification">The modification to apply to each response.</param>
        public void ModifyResponses(Func<Response, Response> modification)
        {
            var modifiedResponses = this.responses.Select(modification).ToList();

            this.responses.Clear();
            this.responses.AddRange(modifiedResponses);
        }

        /// <summary>
        /// Modifies all currently registered autonomous outputs.
        /// </summary>
        /// <param name="modification">The modification to apply to each output.</param>
        public void ModifyAutonomousOutputs(Func<SimpleOutput, SimpleOutput> modification)
        {
            var modifiedOutputs = this.autonomousOutputs.Select(modification).ToList();

            this.autonomousOutputs.Clear();
            this.autonomousOutputs.AddRange(modifiedOutputs);
        }

        /// <summary>
        /// Registers a response.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the response builder.</param>
        public abstract void RegisterResponse(Action<Response.Builder> configureBuilder);

        /// <summary>
        /// Registers an autonomous behaviour output.
        /// </summary>
        /// <param name="configureBuilder">An action used to configure the autonomous output builder.</param>
        public void RegisterAutonomousOutput(Action<SimpleOutput.Builder> configureBuilder)
        {
            var builder = new SimpleOutput.Builder(this.BotContext, this.MessageBuilderFactory);

            configureBuilder(builder);

            this.autonomousOutputs.Add(builder.Build());
        }

        /// <summary>
        /// Stores the response.
        /// </summary>
        /// <param name="response">The response.</param>
        protected void AddResponse(Response response)
        {
            this.responses.Add(response);
        }
    }
}
