using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Relevance;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Core
{
    public class PulseDrivenResponseSelector : IResponseSelector, IDisposable
    {
        private readonly int responseWindow;
        private readonly IPulseDriver pulseDriver;
        private readonly IRelevanceArgumentEvaluator evaluator;
        private readonly ILogger logger;
        private readonly IDictionary<MessageContext, ResponseCandidateSet> responseCandidates;

        public PulseDrivenResponseSelector(int responseWindow, IPulseDriver pulseDriver,
            IRelevanceArgumentEvaluator evaluator, ILogger logger)
        {
            Raise.ArgumentException.If(responseWindow < 1, nameof(responseWindow),
                "The response window cannot be less than 1");

            Raise.ArgumentNullException.IfIsNull(pulseDriver, nameof(pulseDriver));
            Raise.ArgumentNullException.IfIsNull(evaluator, nameof(evaluator));
            Raise.ArgumentNullException.IfIsNull(logger, nameof(logger));

            this.responseWindow = responseWindow;
            this.pulseDriver = pulseDriver;
            this.evaluator = evaluator;
            this.logger = logger.ForContext<PulseDrivenResponseSelector>();
            this.responseCandidates = new Dictionary<MessageContext, ResponseCandidateSet>();

            this.pulseDriver.PulseOccurred += this.OnPulseOccurred;
        }

        public event EventHandler<ResponseSelectedEventArgs> ResponseSelected;

        public event EventHandler<ResponseWindowExpiredEventArgs> ResponseWindowExpired;

        public void InspectVisitor(IBehaviourVisitor visitor)
        {
            var onMessageVisitor = visitor as OnMessageVisitor;

            if (onMessageVisitor != null)
            {
                var key = onMessageVisitor.Message;

                Debug.Assert(!this.responseCandidates.ContainsKey(key),
                    "OnMessageVisitors should carry unique messages");

                this.responseCandidates[key] = new ResponseCandidateSet { PulsesRemaining = this.responseWindow };
                this.logger.Verbose("Created response candidate set for {Message} (matures in {Window} pulses)",
                    key.Body, this.responseWindow);
            }

            // Assign each response registered with the visitor to the appropriate response candidate set.
            foreach (var response in visitor.Responses)
            {
                var key = response.RespondingTo;

                if (!this.responseCandidates.ContainsKey(key))
                {
                    // The window for responding to this particular message has closed.
                    continue;
                }

                this.responseCandidates[key].Responses.Add(response);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.pulseDriver.PulseOccurred -= this.OnPulseOccurred;
        }

        private void OnPulseOccurred(object sender, EventArgs e)
        {
            // Decrement the visitors remaining count for all response candidate sets.
            foreach (var responseCandidateSet in this.responseCandidates.Values)
            {
                responseCandidateSet.PulsesRemaining -= 1;
            }

            // Process the response candidates sets that have matured.
            foreach (var pair in this.responseCandidates.Where(it => it.Value.PulsesRemaining == 0).ToList())
            {
                this.responseCandidates.Remove(pair.Key);

                var respondingTo = pair.Key;
                var candidates = pair.Value.Responses.ToList();

                if (candidates.Any())
                {
                    var chosenResponse = this.Select(candidates, respondingTo);

                    this.logger.Verbose("Response candidate set for {Message} matured - " +
                                        "selected {Response} ({CandidateCount} possibilities)",
                        respondingTo, chosenResponse, candidates.Count);

                    this.OnResponseSelected(chosenResponse);
                }
                else
                {
                    this.logger.Verbose("Response candidate set for {Message} expired (no candidates found)",
                        respondingTo);

                    this.OnResponseWindowExpired(respondingTo);
                }
            }
        }

        private Response Select(IEnumerable<Response> candidates, MessageContext message)
        {
            Debug.Assert(candidates.Any(), "At least one candidate should exist");

            // Assumption: order does not change.
            var arguments = candidates.Select(it => it.RelevanceArgument);
            var scoredArguments = this.evaluator.Evaluate(arguments, message);

            this.logger.Verbose("Evaluated response relevance arguments: {ScoredRelevanceArguments}",
                scoredArguments);

            var query = from response in candidates
                        join arg in scoredArguments on response.RelevanceArgument equals arg.Item1
                        let score = arg.Item2
                        orderby score descending
                        select response;

            return query.First();
        }

        private void OnResponseSelected(Response response)
        {
            this.ResponseSelected?.Invoke(this, new ResponseSelectedEventArgs(response));
        }

        private void OnResponseWindowExpired(MessageContext respondingTo)
        {
            this.ResponseWindowExpired?.Invoke(this, new ResponseWindowExpiredEventArgs(respondingTo));
        }

        private class ResponseCandidateSet
        {
            public ResponseCandidateSet()
            {
                this.Responses = new List<Response>();
            }

            public int PulsesRemaining { get; set; }
            public IList<Response> Responses { get; }
        }
    }
}
