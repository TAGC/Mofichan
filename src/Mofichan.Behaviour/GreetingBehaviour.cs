using System;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.FilterAttributes;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> extends Mofichan with greeting-related functionality.
    /// </summary>
    /// <remarks>
    /// Adding this module to the behaviour chain will allow Mofichan to respond to people greeting her.
    /// </remarks>
    public class GreetingBehaviour : BaseReflectionBehaviour
    {
        private const string IdentityMatch = @"(mofichan|mofi)";
        private const string GreetingWordMatch = @"(hey|hi|hello|sup|yo)";
        private const string GreetingMatch = GreetingWordMatch + @",?\s*" + IdentityMatch + @"\W*";

        private static readonly string[] MofiGreetings = new[] { "Hey", "Hi", "Hello" };
        private static readonly string[] MofiEmotes = new[] { ":3", "^-^", "o/" };
        private static readonly double EmoteChance = 0.7;

        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="GreetingBehaviour"/> class.
        /// </summary>
        public GreetingBehaviour()
        {
            this.random = new Random();
        }

        /// <summary>
        /// Generates a greeting when Mofichan receives one.
        /// </summary>
        /// <param name="message">The received greeting.</param>
        /// <returns>A greeting in response.</returns>
        [RegexIncomingMessageFilter(GreetingMatch, RegexOptions.IgnoreCase)]
        public OutgoingMessage? ReturnGreeting(IncomingMessage message)
        {
            var sender = message.Context.From as IUser;
            var recipient = message.Context.To as IUser;

            return this.ConstructGreetingMessage(mofichan: recipient, target: sender);
        }

        private OutgoingMessage ConstructGreetingMessage(IUser mofichan, IUser target)
        {
            string greeting = MofiGreetings[this.random.Next(MofiGreetings.Length)];
            string emote = string.Empty;

            if (this.random.NextDouble() < EmoteChance)
            {
                emote = string.Concat(" ", MofiEmotes[this.random.Next(MofiEmotes.Length)]);
            }

            var body = string.Format("{0} {1}{2}", greeting, target.Name, emote);
            var context = new MessageContext(from: mofichan, to: target, body: body);
            return new OutgoingMessage { Context = context };
        }
    }
}
