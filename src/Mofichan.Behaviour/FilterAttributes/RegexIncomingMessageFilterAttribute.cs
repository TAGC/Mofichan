using System.Text.RegularExpressions;
using Mofichan.Behaviour.Base;
using Mofichan.Core;

namespace Mofichan.Behaviour.FilterAttributes
{
    /// <summary>
    /// A type of <see cref="BaseIncomingMessageFilterAttribute"/> that filters
    /// incoming messages based on a regular expression.
    /// </summary>
    public class RegexIncomingMessageFilterAttribute : BaseIncomingMessageFilterAttribute
    {
        private readonly Regex regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexIncomingMessageFilterAttribute"/> class.
        /// </summary>
        /// <param name="regex">The regex to match incoming messages against.</param>
        public RegexIncomingMessageFilterAttribute(string regex) : this(regex, RegexOptions.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexIncomingMessageFilterAttribute" /> class.
        /// </summary>
        /// <param name="regex">The regex to match incoming messages against.</param>
        /// <param name="options">The regex options to use when matching.</param>
        public RegexIncomingMessageFilterAttribute(string regex, RegexOptions options)
        {
            this.regex = new Regex(regex, options);
        }

        /// <summary>
        /// Called to notify this observer of an incoming message.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public override void OnNext(IncomingMessage message)
        {
            if (this.regex.IsMatch(message.Context.Body))
            {
                this.SendDownstream(message);
            }
        }
    }
}
