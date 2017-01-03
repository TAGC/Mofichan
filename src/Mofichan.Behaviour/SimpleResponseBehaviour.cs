using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> causes Mofichan to produce simple responses
    /// to messages.
    /// <para></para>
    /// The responses generated have low relevance and will likely lose to other response candidates
    /// if any are made.
    /// </summary>
    public class SimpleResponseBehaviour : BaseBehaviour
    {
        private readonly BotContext botContext;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleResponseBehaviour"/> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        public SimpleResponseBehaviour(BotContext botContext)
        {
            Raise.ArgumentNullException.IfIsNull(botContext, nameof(botContext));

            this.botContext = botContext;
            this.random = new Random();
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
            base.HandleMessageVisitor(visitor);

            var sender = (IUser)visitor.Message.From;
            var tags = visitor.Message.Tags.ToList();
            var numTags = tags.Count;
            var randVal = this.random.NextDouble();

            bool confused = numTags == 1 && tags[0] == "directedAtMofichan";
            bool possibleCompliment = this.DirectedAtMofi(sender, tags) && tags.Contains("positive");
            bool possibleInsult = this.DirectedAtMofi(sender, tags) && tags.Contains("negative");

            if (confused && randVal <= 0.2)
            {
                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromTags(prefix: string.Empty, tags: new[] { "confused" }))
                    .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan")));
            }
            else if (possibleCompliment && !possibleInsult && randVal <= 0.7)
            {
                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromTags(prefix: string.Empty, tags: new[] { "emote,cute,happy" }))
                    .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan", "positive")));
            }
            else if (possibleInsult && !possibleCompliment && randVal <= 0.5)
            {
                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromTags(prefix: string.Empty, tags: new[] { "emote,cute,sad" }))
                    .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan", "negative")));
            }
        }

        private bool DirectedAtMofi(IUser sender, IEnumerable<string> tags)
        {
            return this.botContext.Attention.IsPayingAttentionToUser(sender) ||
                tags.Contains("directedAtMofichan");
        }
    }
}
