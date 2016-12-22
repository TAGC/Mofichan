using System;
using Mofichan.Behaviour.Base;
using Mofichan.Core;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> causes Mofichan to delay her responses.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will make Mofichan seem less bot-like in
    /// her response rate.
    /// </remarks>
    public class DelayBehaviour : BaseBehaviour
    {
        private static readonly TimeSpan FixedDelay = TimeSpan.FromSeconds(0.4);
        private static readonly double WordsPerMinute = 500;
        private static readonly double DelayStandardDeviation = 1.05;

        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayBehaviour" /> class.
        /// </summary>
        public DelayBehaviour()
        {
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
            this.DelayResponses(visitor);
        }

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnPulseVisitor" />.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected override void HandlePulseVisitor(OnPulseVisitor visitor)
        {
            base.HandlePulseVisitor(visitor);
            this.DelayResponses(visitor);
        }

        private static int CountWordsInString(string input)
        {
            char[] delimiters = new[] { ' ', '\r', '\n' };
            return input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private void DelayResponses(IBehaviourVisitor visitor)
        {
            visitor.ModifyResponses(response =>
            {
                if (response.Message == null)
                {
                    return response;
                }

                var message = response.Message;
                TimeSpan delay = this.CalculateDelayForMessage(message.Body);
                var delayedMessage = new MessageContext(message.From, message.To, message.Body, delay);

                return response.DeriveFromNewMessage(delayedMessage);
            });
        }

        private TimeSpan CalculateDelayForMessage(string messageBody)
        {
            double numWords = CountWordsInString(messageBody);

            double fixedDelay = FixedDelay.Milliseconds;
            double variableDelay = TimeSpan.FromMinutes(numWords / WordsPerMinute).TotalMilliseconds;

            double delay = this.random.SampleGaussian(fixedDelay + variableDelay, DelayStandardDeviation);

            return TimeSpan.FromMilliseconds(delay);
        }
    }
}
