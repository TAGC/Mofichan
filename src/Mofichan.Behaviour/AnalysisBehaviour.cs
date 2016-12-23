using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> augments Mofichan with a function for testing how Mofichan
    /// classifiers particular phrases.
    /// </summary>
    public class AnalysisBehaviour : BaseBehaviour
    {
        private static readonly string PerformAnalysisCommand =
            "(perform analysis|analyse( phrase)?).*\"(?<phrase>.+)\"";

        private readonly IAttentionManager attentionManager;
        private readonly Func<IMessageClassifier> messageClassifierFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisBehaviour"/> class.
        /// </summary>
        /// <param name="attentionManager">The attention manager.</param>
        /// <param name="messageClassifierFactory">A factory for producing message classifiers.</param>
        public AnalysisBehaviour(IAttentionManager attentionManager, Func<IMessageClassifier> messageClassifierFactory)
        {
            Raise.ArgumentNullException.IfIsNull(attentionManager, nameof(attentionManager));
            Raise.ArgumentNullException.IfIsNull(messageClassifierFactory, nameof(messageClassifierFactory));

            this.attentionManager = attentionManager;
            this.messageClassifierFactory = messageClassifierFactory;
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

            var tags = visitor.Message.Tags;
            var messageBody = visitor.Message.Body;
            var user = visitor.Message.From as IUser;
            Debug.Assert(user != null, "The message should be from a user");

            var hasAttention = this.attentionManager.IsPayingAttentionToUser(user)
                || tags.Contains("directedAtMofichan");

            var match = Regex.Match(messageBody, PerformAnalysisCommand, RegexOptions.IgnoreCase);

            if (hasAttention && match.Success)
            {
                var phrase = match.Groups["phrase"].Value;
                var classifications = this.messageClassifierFactory().Classify(phrase);

                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => ConfigureMessage(mb, classifications))
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                    .RelevantBecause(it => it.GuaranteesRelevance()));
            }
        }

        private static void ConfigureMessage(IResponseBodyBuilder builder, IEnumerable<string> classifications)
        {
            if (!classifications.Any())
            {
                builder.FromAnyOf(prefix: string.Empty, phrases: new[]
                {
                    "I didn't get any classifications",
                    "I didn't classify it as anything"
                });

                return;
            }

            builder.FromAnyOf(prefix: string.Empty, phrases: new[]
            {
                "Here's my guess: ",
                "Here's what I think: ",
                "I got this: ",
            });

            builder.FromRaw(string.Join(", ", classifications.Select(it => "#" + it)));
        }
    }
}
