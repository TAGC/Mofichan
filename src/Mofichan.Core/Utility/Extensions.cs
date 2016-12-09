using System;
using System.Collections.Generic;

namespace Mofichan.Core.Utility
{
    /// <summary>
    /// Provides useful general extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Forms an outgoing message with a specified body in response to an incoming.
        /// </summary>
        /// <param name="incomingMessage">The incoming message to form a reply to.</param>
        /// <param name="replyBody">The body of the reply.</param>
        /// <returns>The generated reply.</returns>
        public static MessageContext Reply(this MessageContext incomingMessage, string replyBody)
        {
            var from = incomingMessage.From;
            var to = incomingMessage.To;

            return new MessageContext(from: to, to: from, body: replyBody);
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

        /// <summary>
        /// Samples from a Gaussian distribution using a <c>Random</c> object to generate
        /// samples from a uniform distribution to convert into Gaussian distribution samples.
        /// </summary>
        /// <param name="random">The random object.</param>
        /// <param name="mu">The mean of the normal distribution.</param>
        /// <param name="sigma">The standard deviation of the normal distribution.</param>
        /// <returns>The sampled random value.</returns>
        public static double SampleGaussian(this Random random, double mu, double sigma)
        {
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return (y1 * sigma) + mu;
        }
    }
}
