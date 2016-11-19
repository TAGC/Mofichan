using System;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Behaviour.FilterAttributes;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using static Mofichan.Core.Utility.Constants;

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
        private const string GreetingWord = @"(hey|hi|hello|sup|yo)";
        private const string GreetingMatch = GreetingWord + @",?\s*" + IdentityMatch + @"\W*";

        private const string WellbeingQueries = @"(how are you|how r u|you alright)";

        private static readonly string[] MofiGreetings = new[] { "Hey", "Hi", "Hello" };
        private static readonly string[] WellbeingResponses = new[] { "I'm good thanks", "I'm okay ty" };

        private static readonly string[] MofiWellbeingEmotes = new[] { ":3", "^-^" };
        private static readonly string[] MofiGreetingEmotes = new[] { ":3", "^-^", "o/" };
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

        [RegexIncomingMessageFilter(IdentityMatch, RegexOptions.IgnoreCase)]
        [RegexIncomingMessageFilter(WellbeingQueries, RegexOptions.IgnoreCase)]
        public OutgoingMessage? RespondToWellbeingQuery(IncomingMessage message)
        {
            var sender = message.Context.From as IUser;

            string wellbeingResponse = WellbeingResponses[this.random.Next(WellbeingResponses.Length)];
            string emote = string.Empty;

            if (this.random.NextDouble() < EmoteChance)
            {
                emote = string.Concat(" ", MofiWellbeingEmotes[this.random.Next(MofiWellbeingEmotes.Length)]);
            }

            var body = string.Format("{0} {1}{2}", wellbeingResponse, sender.Name, emote);
            var context = new MessageContext(from: null, to: sender, body: body);
            return new OutgoingMessage { Context = context };
        }

        private OutgoingMessage ConstructGreetingMessage(IUser mofichan, IUser target)
        {
            string greeting = MofiGreetings[this.random.Next(MofiGreetings.Length)];
            string emote = string.Empty;

            if (this.random.NextDouble() < EmoteChance)
            {
                emote = string.Concat(" ", MofiGreetingEmotes[this.random.Next(MofiGreetingEmotes.Length)]);
            }

            var body = string.Format("{0} {1}{2}", greeting, target.Name, emote);
            var context = new MessageContext(from: mofichan, to: target, body: body);
            return new OutgoingMessage { Context = context };
        }
    }
}
