using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.BotState;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Serilog;
using static Mofichan.Core.Flow.AutoFlowManager;
using LearnAnalysisCallback = System.Action<
    Mofichan.Core.Visitor.IBehaviourVisitor,
    Mofichan.Core.MessageContext,
    string,
    System.Collections.Generic.IEnumerable<string>>;

namespace Mofichan.Behaviour.Learning
{
    internal class AutoAnalysisBehaviour : BaseFlowReflectionBehaviour
    {
        #region Nodes
        private const string S0 = "S0";
        private const string S1 = "S1";
        private const string S2 = "S2";
        private const string S3 = "S3";
        private const string S4 = "S4";
        #endregion

        #region Transitions
        private const string T0_1 = "T0,1";
        private const string T1_2 = "T1,2";
        private const string T2_1 = "T2,1";
        private const string T2_3 = "T2,3";
        private const string T3_1 = "T3,1";
        private const string T3_4 = "T3,4";
        private const string T4_1 = "T4,1";
        #endregion

        private static readonly int MaxAutoAnalysisCandidates = 1;  // 5;
        private static readonly int AutoAnalysisPulseMean = 150;  // 3000;
        private static readonly int AutoAnalysisPulseStdDev = 10;  // 1000;

        private readonly LearnAnalysisCallback learnAnalysis;
        private readonly Random random;
        private readonly Queue<MessageContext> autoAnalysisCandidates;

        public AutoAnalysisBehaviour(LearnAnalysisCallback learnAnalysis, BotContext botContext, ILogger logger)
            : base(S0, botContext, logger)
        {
            this.learnAnalysis = learnAnalysis;
            this.random = new Random();
            this.autoAnalysisCandidates = new Queue<MessageContext>();

            this.RegisterSimpleTransition(T0_1, from: S0, to: S1);
            this.RegisterSimpleTransition(T1_2, from: S1, to: S2);
            this.RegisterSimpleTransition(T2_1, from: S2, to: S1);
            this.RegisterSimpleTransition(T2_3, from: S2, to: S3);
            this.RegisterSimpleTransition(T3_1, from: S3, to: S1);
            this.RegisterSimpleTransition(T3_4, from: S3, to: S4);
            this.RegisterSimpleTransition(T4_1, from: S4, to: S1);
            this.Configure<AutoFlow>(Create);
        }

        [FlowState(id: S0)]
        public FlowNodeState? BeforeAdminFound(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
        {
            manager.MakeTransitionsImpossible();

            if (context.Message == null)
            {
                return null;
            }

            var user = context.Message.From as IUser;
            Debug.Assert(user != null, "Message should be from user");

            if (user.Type == UserType.Adminstrator)
            {
                context.Extras.Admin = user;
                context.Extras.RenewWait = true;
                manager.MakeTransitionCertain(T0_1);
                return FlowNodeState.Dormant;
            }

            return null;
        }

        [FlowState(id: S1)]
        public void Idle(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
        {
            /*
             * We adopt this approach instead of making the node go dormant
             * so that this flow will continue to consume (and ignore) messages
             * during its idle state.
             * 
             * Otherwise, these messages will clog the backlog and be erroneously fed
             * to nodes further along the flow.
             */ 
            if (context.Extras.RenewWait)
            {
                manager.MakeTransitionsImpossible();
                manager[T1_2] = (int)this.random.SampleGaussian(AutoAnalysisPulseMean, AutoAnalysisPulseStdDev);
                context.Extras.RenewWait = false;
            }
        }

        [FlowState(id: S2)]
        public FlowNodeState? AttemptAutoAnalysis(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
        {
            context.Extras.RenewWait = true;
            manager.MakeTransitionsImpossible();

            if (!this.autoAnalysisCandidates.Any())
            {
                manager.MakeTransitionCertain(T2_1);
                return null;
            }

            var candidate = this.autoAnalysisCandidates.Dequeue();
            var candidateFrom = candidate.From as IUser;
            var candidateBody = candidate.Body;
            var hashTags = string.Join(", ", candidate.Tags.Select(it => "#" + it));
            var admin = context.Extras.Admin as IUser;

            Debug.Assert(!string.IsNullOrEmpty(hashTags), "The candidate message should have at least one tag");
            Debug.Assert(candidateFrom != null, "The candidate message should be from a user");
            Debug.Assert(admin != null && admin.Type == UserType.Adminstrator,
                "The original message should be from an admin");

            context.Extras.AnalysisBody = candidateBody;
            context.Extras.AnalysisTags = candidate.Tags;

            var candidateFromName = admin.Name == candidateFrom.Name ? "you" : candidateFrom.Name;

            visitor.RegisterAutonomousOutput(aob => aob
                .WithMessage(recipient: admin, configureBuilder: mb => mb
                    .FromFormatted("@{0}, I tried analysing a message from {1}: ", admin.Name, candidateFromName)
                    .FromFormatted("\"{0}\": {1}.", candidateBody, hashTags)
                    .FromAnyOf("Did I get it right?", "Is this right?"))
                .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(admin))
                .WithSideEffect(() => manager.MakeTransitionCertain(T2_3)));

            return FlowNodeState.Dormant;
        }

        [FlowState(id: S3)]
        public FlowNodeState? AwaitingAnalysisConfirmation(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
        {
            manager.MakeTransitionsImpossible();
            
            if (!context.Extras.Admin.Equals(context.Message?.From))
            {
                manager[T3_1] = this.GenerateTimeout();
                return null;
            }

            var tags = context.Message.Tags;
            var admin = context.Extras.Admin as IUser;

            bool twoTags = tags.Count() == 2;
            bool oneTag = tags.Count() == 1;
            bool directedAtMofichan = tags.Contains("directedAtMofichan");
            bool payingAttentionToAdmin = this.BotContext.Attention.IsPayingAttentionToUser(admin);
            Predicate<string> matchesTag = tag => (twoTags && directedAtMofichan && tags.Contains(tag)) ||
                                                  (oneTag && tags.Contains(tag) && payingAttentionToAdmin);

            bool abort = context.Message.Body.ToLowerInvariant().Contains("skip");
            bool affirmation = matchesTag("affirmation") && !abort;
            bool refutation = matchesTag("refutation") && !abort;

            if (!payingAttentionToAdmin)
            {
                manager.MakeTransitionCertain(T3_1);
                return null;
            }
            else
            {
                manager[T3_1] = this.GenerateTimeout();
            }

            if (affirmation)
            {
                string analysisBody = context.Extras.AnalysisBody;
                IEnumerable<string> analysisTags = context.Extras.AnalysisTags;

                this.learnAnalysis(visitor, context.Message, analysisBody, analysisTags);

                return FlowNodeState.Dormant;
            }
            else if (refutation)
            {
                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb
                        .FromAnyOf("Oh okay. ", "My mistake. ", "Oops. ")
                        .FromRaw("What should it be?"))
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(admin))
                    .WithSideEffect(() => manager.MakeTransitionCertain(T3_4))
                    .RelevantBecause(it => it.GuaranteesRelevance()));

                return FlowNodeState.Dormant;
            }
            else if (abort)
            {
                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb
                        .FromAnyOf("Okay.", "No problem.")
                        .FromAnyOf("I'll skip this one.", "I'll skip it.", "Skipping."))
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(admin))
                    .WithSideEffect(() => manager.MakeTransitionCertain(T3_1))
                    .RelevantBecause(it => it.GuaranteesRelevance()));

                return FlowNodeState.Dormant;
            }

            return null;
        }

        [FlowState(id: S4)]
        public FlowNodeState? AwaitingCorrectedAnalysis(FlowContext context, FlowTransitionManager manager, IBehaviourVisitor visitor)
        {
            manager.MakeTransitionsImpossible();

            if (!context.Extras.Admin.Equals(context.Message?.From))
            {
                manager[T4_1] = this.GenerateTimeout();
                return null;
            }

            var admin = context.Extras.Admin as IUser;
            var payingAttentionToAdmin = this.BotContext.Attention.IsPayingAttentionToUser(admin);
            var match = Regex.Match(context.Message.Body, "(\\s*#[\\w]*)+", RegexOptions.IgnoreCase);
            bool abort = context.Message.Body.ToLowerInvariant().Contains("skip");

            if (!payingAttentionToAdmin)
            {
                manager.MakeTransitionCertain(T4_1);
                return null;
            }
            else if (abort)
            {
                visitor.RegisterResponse(rb => rb
                    .To(context.Message)
                    .WithMessage(mb => mb
                        .FromAnyOf("Okay. ", "No problem. ")
                        .FromAnyOf("I'll skip this one.", "I'll skip it."))
                    .WithBotContextChange(ctx => ctx.Attention.RenewAttentionTowardsUser(admin))
                    .WithSideEffect(() => manager.MakeTransitionCertain(T4_1))
                    .RelevantBecause(it => it.GuaranteesRelevance()));

                return FlowNodeState.Dormant;
            }
            else if (!match.Success)
            {
                manager[T4_1] = this.GenerateTimeout();
            }

            var correctedTags = match.Value
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                .Select(it => it.Trim('#'));

            if (correctedTags.Any())
            {
                string analysisBody = context.Extras.AnalysisBody;
                this.learnAnalysis(visitor, context.Message, analysisBody, correctedTags);
                manager.MakeTransitionCertain(T4_1);
            }

            return null;
        }

        protected override void HandleMessageVisitor(OnMessageVisitor visitor)
        {
            /*
             * Messages are stored as analysis candidates.
             * 
             * Only messages that have at least one possible classification are stored.
             * Nonsense messages or messages well beyond the scope of what Mofichan
             * is currently capable of handling will be ignored and should manually be
             * taught to her if necessary.
             */
            var message = visitor.Message;
            var classifications = message.Tags;

            if (!this.autoAnalysisCandidates.Contains(message) && classifications.Any())
            {
                while (this.autoAnalysisCandidates.Count >= MaxAutoAnalysisCandidates)
                {
                    this.autoAnalysisCandidates.Dequeue();
                }

                this.autoAnalysisCandidates.Enqueue(message);

                Debug.Assert(this.autoAnalysisCandidates.Count <= MaxAutoAnalysisCandidates,
                    "The maximum limit for analysis candidates was exceeded");

                Debug.Assert(this.autoAnalysisCandidates.Distinct().Count() == this.autoAnalysisCandidates.Count,
                    "There are duplicate analysis candidates");
            }

            base.HandleMessageVisitor(visitor);
        }

        private int GenerateTimeout()
        {
            return (int)this.random.SampleGaussian(200, 10);
        }
    }
}
