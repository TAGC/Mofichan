using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
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
    public class GreetingBehaviour : BaseBehaviour
    {
        private static readonly string[] MofiGreetings = new[] { "Hey", "Hi", "Hello" };
        private static readonly string[] MofiEmotes = new[] { ":3", "^-^", "o/" };
        private static readonly string[] UserGreetings = MofiGreetings.Concat(new[] { "Sup", "Yo" }).ToArray();

        private static readonly double EmoteChance = 0.7;

        private readonly Regex greetingPattern;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="GreetingBehaviour"/> class.
        /// </summary>
        public GreetingBehaviour()
        {
            // TODO: refactor to inject identity match logic.
            var identityMatch = @"(mofichan|mofi)";

            var greetingMatch = string.Concat("(", string.Join("|", UserGreetings), ")");

            this.greetingPattern = new Regex(string.Format(@"{0},?\s*{1}\W*", greetingMatch, identityMatch),
                RegexOptions.IgnoreCase);

            this.random = new Random();
        }

        /// <summary>
        /// Determines whether this instance can process the specified incoming message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the incoming message; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            return this.IsGreetingForMofichan(message);
        }

        /// <summary>
        /// Determines whether this instance can process the specified outgoing message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the outgoing messagee; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the incoming message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleIncomingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            Debug.Assert(this.IsGreetingForMofichan(message),
                "The message should definitely be a greeting for Mofichan");

            var sender = message.Context.From as IUser;
            var recipient = message.Context.To as IUser;

            var outgoingMessage = this.ConstructGreetingMessage(
                mofichan: recipient, target: sender);

            this.SendUpstream(outgoingMessage);
        }

        /// <summary>
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleOutgoingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        private bool IsGreetingForMofichan(IncomingMessage message)
        {
            var sender = message.Context.From as IUser;

            var senderIsUser = sender != null;
            var senderIsNotSelf = sender.Type != UserType.Self;
            var messageIsGreeting = this.greetingPattern.IsMatch(message.Context.Body);

            return senderIsUser && senderIsNotSelf && messageIsGreeting;
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
