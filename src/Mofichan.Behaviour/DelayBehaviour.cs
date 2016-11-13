using System;
using System.Threading.Tasks;
using Mofichan.Behaviour.Base;
using Mofichan.Core;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> causes Mofichan to delay her responses.
    /// <para></para>
    /// Adding this module to the behaviour chain will make Mofichan seem less bot-like in
    /// her response rate.
    /// </summary>
    public class DelayBehaviour : BaseBehaviour
    {
        private static readonly TimeSpan FixedDelay = TimeSpan.FromSeconds(0.4);
        private static readonly double WordsPerMinute = 500;
        private static readonly double DelayStandardDeviation = 1.05;

        private readonly Random random;

        public DelayBehaviour()
        {
            this.random = new Random();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            return true;
        }

        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            var context = message.Context;
            TimeSpan delay = CalculateDelayForMessage(message.Context.Body);
            var newMessageContext = new MessageContext(context.From, context.To, context.Body, delay);
            var newMessage = new OutgoingMessage { Context = newMessageContext };

            this.SendUpstream(newMessage);
        }

        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            return false;
        }

        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            throw new NotImplementedException();
        }

        private async Task SendDelayedMessageAsync(OutgoingMessage message)
        {
            TimeSpan delay = CalculateDelayForMessage(message.Context.Body);
            await Task.Delay(delay);

            this.SendUpstream(message);
        }

        private TimeSpan CalculateDelayForMessage(string messageBody)
        {
            double numWords = CountWordsInString(messageBody);

            double fixedDelay = FixedDelay.Milliseconds;
            double variableDelay = TimeSpan.FromMinutes(numWords / WordsPerMinute).TotalMilliseconds;

            double delay = SampleGaussian(this.random, fixedDelay + variableDelay, DelayStandardDeviation);

            return TimeSpan.FromMilliseconds(delay);
        }

        private static int CountWordsInString(string input)
        {
            char[] delimiters = new char[] { ' ', '\r', '\n' };
            return input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static double SampleGaussian(Random random, double mean, double standardDeviation)
        {
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * standardDeviation + mean;
        }
    }
}
