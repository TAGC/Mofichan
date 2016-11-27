using System.Collections.Generic;

namespace Mofichan.Core.Utility
{
    /// <summary>
    /// Provides useful general extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Forms an <see cref="OutgoingMessage"/> in response to a(n) <see cref="MessageContext"/>
        /// with a specified body.
        /// </summary>
        /// <param name="messageContext">The message context to form a reply to.</param>
        /// <param name="replyBody">The body of the reply.</param>
        /// <returns>The generated reply.</returns>
        public static OutgoingMessage Reply(this MessageContext messageContext, string replyBody)
        {
            var from = messageContext.From;
            var to = messageContext.To;

            var replyContext = new MessageContext(from: to, to: from, body: replyBody);

            return new OutgoingMessage { Context = replyContext };
        }

        /// <summary>
        /// Tries the get a value from a dictionary and returns the default value
        /// if the key is not found.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary to query.</param>
        /// <param name="key">The key to try to retrieve the value of.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>
        /// The value corresponding to <paramref name="key"/> if found;
        /// otherwise, <paramref name="defaultValue"/>.
        /// </returns>
        public static TValue TryGetValueWithDefault<TValue, TKey>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
