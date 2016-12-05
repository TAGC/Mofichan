using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mofichan.DataAccess.Response
{
    /// <summary>
    /// Represents objects that can be used to resolve context-dependent placeholders within an article.
    /// </summary>
    internal interface IArticleResolver
    {
        /// <summary>
        /// Resolves the context-dependent placeholders within an article by attempting to
        /// replace them with information provided by the context.
        /// </summary>
        /// <param name="article">The article to resolve.</param>
        /// <param name="context">A dynamically-typed context to resolve the article with.</param>
        /// <returns>
        /// The article with all context-dependent placeholders replaced with information from
        /// <paramref name="context"/>
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if any part of <paramref name="article"/> cannot be resolved.
        /// </exception>
        string Resolve(string article, dynamic context);
    }

    internal class ArticleResolver : IArticleResolver
    {
        private static readonly Regex PlaceholderMatcher = new Regex(@"\${(?<field>\w*?)([.](?<field>\w*?))*}");

        public string Resolve(string article, dynamic context)
        {
            // Iterate until all placeholders are resolved.
            while (true)
            {
                var match = PlaceholderMatcher.Match(article);

                if (!match.Success)
                {
                    return article;
                }

                dynamic currentContext = context;

                foreach (var capture in match.Groups["field"].Captures.OfType<Capture>())
                {
                    string field = capture.Value;

                    try
                    {
                        currentContext = ((IDictionary<string, object>)currentContext)[field];
                    }
                    catch (KeyNotFoundException e)
                    {
                        var message = string.Format("Cannot resolve {0} within {1} using provided context",
                            match.Value, context);
                        throw new ArgumentException(message, e);
                    }
                }

                article = article.Replace(match.Value, currentContext as string);
            }
        }
    }
}
