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
    /// <para></para>
    /// Adding this module to the behaviour chain will allow Mofichan to respond to people greeting her.
    /// </summary>
    public class GreetingBehaviour : BaseBehaviour
    {
        private static readonly string[] MofiGreetings = new[] { "Hey", "Hi", "Hello" };
        private static readonly string[] MofiEmotes = new[] { ":3", "^-^", "o/" };
        private static readonly string[] UserGreetings = MofiGreetings.Concat(new[] { "Sup", "Yo" }).ToArray();

        private static readonly double EmoteChance = 0.7;

        private readonly Regex greetingPattern;
        private readonly Random random;

        public GreetingBehaviour()
        {
            // TODO: refactor to inject identity match logic.
            var identityMatch = @"(mofichan|mofi)";

            var greetingMatch = string.Concat("(", string.Join("|", UserGreetings), ")");

            this.greetingPattern = new Regex(string.Format(@"{0},?\s*{1}\W*", greetingMatch, identityMatch),
                RegexOptions.IgnoreCase);

            this.random = new Random();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            return this.IsGreetingForMofichan(message);
        }

        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            Debug.Assert(this.IsGreetingForMofichan(message));

            var sender = message.Context.From as IUser;
            var recipient = message.Context.To as IUser;

            var outgoingMessage = this.ConstructGreetingMessage(
                mofichan: recipient, target: sender);

            this.SendUpstream(outgoingMessage);
        }

        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        private bool IsGreetingForMofichan(IncomingMessage message)
        {
            var sender = message.Context.From as IUser;
            var recipient = message.Context.To as IUser;

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
