using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core.BotState;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using Serilog;
using static Mofichan.Core.Flow.UserDrivenFlowManager;
using LearnAnalysisCallback = System.Action<
Mofichan.Core.Visitor.IBehaviourVisitor,
Mofichan.Core.MessageContext,
string,
System.Collections.Generic.IEnumerable<string>>;

namespace Mofichan.Behaviour.Learning
{
    internal class LearnAnalysisBehaviour : BaseFlowReflectionBehaviour
    {
        private static readonly string LearnAnalysisCommand = "learn analysis.*\"(?<body>.+)\"(?<tags>(\\s*#[\\w]*)+)";
        private readonly LearnAnalysisCallback learnAnalysis;

        public LearnAnalysisBehaviour(LearnAnalysisCallback learnAnalysis, BotContext botContext, ILogger logger)
            : base("S0", botContext, logger)
        {
            this.learnAnalysis = learnAnalysis;

            this.RegisterSimpleNode("STerm");
            this.RegisterAttentionGuardNode("S0", "T0,1", "T0,Term");
            this.RegisterSimpleTransition("T0,1", from: "S0", to: "S1");
            this.RegisterSimpleTransition("T0,Term", from: "S0", to: "STerm");
            this.RegisterSimpleTransition("T1,Term", from: "S1", to: "STerm");
            this.Configure<UserDrivenFlow>(Create);
        }

        /// <summary>
        /// Represents the state while a user holds Mofichan's attention.
        /// </summary>
        /// <param name="context">The flow context.</param>
        /// <param name="manager">The transition manager.</param>
        /// <param name="visitor">The visitor.</param>
        [FlowState(id: "S1")]
        public void WithAttention(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
        {
            var messageBody = context.Message.Body;
            var tags = context.Message.Tags;
            var user = context.Message.From as IUser;
            Debug.Assert(user != null, "Message should be from user");

            bool authorised = user.Type == UserType.Adminstrator;
            var learnAnalysisMatch = Regex.Match(messageBody, LearnAnalysisCommand, RegexOptions.IgnoreCase);

            if (learnAnalysisMatch.Success && authorised)
            {
                var analysisBody = learnAnalysisMatch.Groups["body"].Value;
                var analysisTags = learnAnalysisMatch.Groups["tags"].Value.Split((char[])null,
                    StringSplitOptions.RemoveEmptyEntries);

                var trimmedTags = analysisTags.Select(it => it.Trim('#'));

                this.learnAnalysis(visitor, context.Message, analysisBody, trimmedTags);
            }
            else if (learnAnalysisMatch.Success)
            {
                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithSideEffect(() => { throw new MofichanAuthorisationException(context.Message); })
                    .RelevantBecause(it => it.GuaranteesRelevance()));
            }

            manager.MakeTransitionCertain("T1,Term");
        }
    }
}
