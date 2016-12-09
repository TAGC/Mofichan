using System.Diagnostics;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// A type of <see cref="IMofichanBehaviour"/> that is responsible for managing
    /// her attention to particular users.
    /// </summary>
    public sealed class AttentionBehaviour : BaseBehaviour
    {
        private static readonly string AttentionPattern = @"^\s*" + Constants.IdentityMatch + @"[\s?!.]*$";
        private readonly IAttentionManager attentionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttentionBehaviour" /> class.
        /// </summary>
        /// <param name="attentionManager">The attention manager.</param>
        public AttentionBehaviour(IAttentionManager attentionManager)
        {
            this.attentionManager = attentionManager;
        }

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnMessageVisitor" />.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected override void HandleMessageVisitor(OnMessageVisitor visitor)
        {
            if (CanHandleIncomingMessage(visitor.Message))
            {
                var sender = visitor.Message.From as IUser;

                Debug.Assert(sender != null, "The message sender should be a user");

                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromAnyOf("hm?", "yes?", "hi?")
                        .FromTags("emote,inquisitive"))
                    .WithSideEffect(() => this.attentionManager.RenewAttentionTowardsUser(sender))
                    .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan")));
            }

            base.HandleMessageVisitor(visitor);
        }

        private static bool CanHandleIncomingMessage(MessageContext message)
        {
            var messageBody = message.Body;

            bool senderIsUser = message.From is IUser;
            bool attentionMatch = Regex.IsMatch(messageBody, AttentionPattern, RegexOptions.IgnoreCase);

            return senderIsUser && attentionMatch;
        }
    }
}
