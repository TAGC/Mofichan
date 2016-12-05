using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// A type of <see cref="IMofichanBehaviour"/> that is responsible for managing
    /// her attention to particular users.
    /// </summary>
    public sealed class AttentionBehaviour : BaseBehaviour
    {
        private static readonly string AttentionPattern = @"^\s*" + Constants.IdentityMatch + @"[\s?!.]*$";
        private readonly IFlowManager flowManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttentionBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">The response builder factory.</param>
        /// <param name="flowManager">The flow manager.</param>
        public AttentionBehaviour(Func<IResponseBuilder> responseBuilderFactory, IFlowManager flowManager)
            : base(responseBuilderFactory)
        {
            this.flowManager = flowManager;
        }

        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            var messageBody = message.Context.Body;

            bool senderIsUser = message.Context.From is IUser;
            bool attentionMatch = Regex.IsMatch(messageBody, AttentionPattern, RegexOptions.IgnoreCase);

            return senderIsUser && attentionMatch;
        }

        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            return false;
        }

        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            var sender = message.Context.From as IUser;

            Debug.Assert(sender != null, "The message sender should be a user");

            var responseBody = this.ResponseBuilder
                .UsingContext(message.Context)
                .FromAnyOf("hm?", "yes?", "hi?")
                .FromTags("emote,inquisitive")
                .Build();

            var responseContext = new MessageContext(from: null, to: sender, body: responseBody);
            var response = new OutgoingMessage { Context = responseContext };

            this.SendUpstream(response);
            this.flowManager.Attention.RenewAttentionTowardsUser(sender);
        }

        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
