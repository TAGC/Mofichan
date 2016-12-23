using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.BotState;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Serilog;

namespace Mofichan.Behaviour.Learning
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> augments Mofichan with the capacity to learn new information.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will allow administrators to teach Mofichan new analysis
    /// information.
    /// <para></para>
    /// It also causes Mofichan to independently try to learn new analysis information.
    /// </remarks>
    public class LearningBehaviour : BaseMultiBehaviour
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningBehaviour"/> class.
        /// </summary>
        /// <param name="botContext">The bot context.</param>
        /// <param name="chainBuilder">The object to use for composing sub-behaviours into a chain.</param>
        /// <param name="logger">The logger to use.</param>
        public LearningBehaviour(
            BotContext botContext,
            IBehaviourChainBuilder chainBuilder,
            ILogger logger)
            : base(chainBuilder,
                  new LearnAnalysisBehaviour(LearnAnalysis, botContext, logger),
                  new AutoAnalysisBehaviour(LearnAnalysis, botContext, logger))
        {
        }

        private static void LearnAnalysis(IBehaviourVisitor visitor, MessageContext respondingTo,
            string analysisBody, IEnumerable<string> analysisTags)
        {
            var user = respondingTo.From as IUser;

            var hashtags = analysisTags.Select(it => "#" + it);

            visitor.RegisterResponse(rb => rb
                .To(respondingTo)
                .WithMessage(mb => mb
                    .FromRaw("Saving new analysis: \"")
                    .FromRaw(analysisBody)
                    .FromRaw("\" with tags: ")
                    .FromRaw(string.Join(", ", hashtags))
                    .FromTags("cute,emote"))
                .WithBotContextChange(ctx => ctx.Memory.SaveAnalysis(analysisBody, analysisTags))
                .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(user))
                .RelevantBecause(it => it.GuaranteesRelevance()));
        }
    }
}
