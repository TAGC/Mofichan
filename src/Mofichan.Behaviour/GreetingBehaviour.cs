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

        /// <summary>
        /// Initializes a new instance of the <see cref="GreetingBehaviour"/> class.
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder"/>.</param>
        /// </summary>
        public GreetingBehaviour(Func<IResponseBuilder> responseBuilderFactory) : base(responseBuilderFactory)
        {
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

            var responseBody = this.ResponseBuilder
                .UsingContext(message.Context)
                .FromTags(prefix: string.Empty, tags: new[] { "greeting,phrase" })
                .FromTags("emote,greeting", "emote,cute")
                .Build();

            var context = new MessageContext(from: null, to: sender, body: responseBody);

            return new OutgoingMessage { Context = context };
        }

        [RegexIncomingMessageFilter(IdentityMatch, RegexOptions.IgnoreCase)]
        [RegexIncomingMessageFilter(WellbeingQueries, RegexOptions.IgnoreCase)]
        public OutgoingMessage? RespondToWellbeingQuery(IncomingMessage message)
        {
            var sender = message.Context.From as IUser;

            var responseBody = this.ResponseBuilder
                .UsingContext(message.Context)
                .FromTags(prefix: string.Empty, tags: new[] { "wellbeing-response,phrase" })
                .FromTags("emote,happy", "emote,cute")
                .Build();

            var context = new MessageContext(from: null, to: sender, body: responseBody);

            return new OutgoingMessage { Context = context };
        }
    }
}
